using System;
using System.Collections.Generic;
using System.Threading;

namespace STCore.Other
{
    /// <summary>
    /// 资源互斥锁管理器
    /// 解决多工位共享硬件资源的冲突问题
    /// </summary>
    public class ResourceLockManager
    {
        #region 单例模式
        public static ResourceLockManager Instance { get; } = new ResourceLockManager();
        private ResourceLockManager() { }
        #endregion

        #region 私有字段
        private readonly Dictionary<string, object> _resourceLocks = new Dictionary<string, object>();
        private readonly Dictionary<string, string> _lockHolders = new Dictionary<string, string>();
        private readonly object _dictLock = new object();
        #endregion

        #region 公共方法
        /// <summary>
        /// 尝试获取资源锁
        /// </summary>
        /// <param name="resourceName">资源名称（如"机械手"、"扫码枪1"）</param>
        /// <param name="holderName">锁持有者名称（一般填工位名称）</param>
        /// <param name="timeoutMs">超时时间（毫秒）</param>
        /// <returns>是否获取成功</returns>
        public bool TryLock(string resourceName, string holderName, int timeoutMs = 1000)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentNullException(nameof(resourceName));
            if (string.IsNullOrWhiteSpace(holderName))
                throw new ArgumentNullException(nameof(holderName));

            lock (_dictLock)
            {
                if (!_resourceLocks.ContainsKey(resourceName))
                {
                    _resourceLocks.Add(resourceName, new object());
                }
            }

            object lockObj = _resourceLocks[resourceName];

            if (Monitor.TryEnter(lockObj, timeoutMs))
            {
                lock (_dictLock)
                {
                    _lockHolders[resourceName] = holderName;
                }
                STLog.Debug($"[{holderName}] 成功获取资源锁：{resourceName}");
                return true;
            }

            string currentHolder;
            lock (_dictLock)
            {
                _lockHolders.TryGetValue(resourceName, out currentHolder);
            }
            STLog.Warn($"[{holderName}] 获取资源锁失败：{resourceName}，当前持有者：{currentHolder ?? "未知"}");
            return false;
        }

        /// <summary>
        /// 释放资源锁
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="holderName">锁持有者名称</param>
        public void Unlock(string resourceName, string holderName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentNullException(nameof(resourceName));
            if (string.IsNullOrWhiteSpace(holderName))
                throw new ArgumentNullException(nameof(holderName));

            lock (_dictLock)
            {
                if (!_resourceLocks.TryGetValue(resourceName, out object lockObj))
                {
                    STLog.Warn($"尝试释放不存在的资源锁：{resourceName}");
                    return;
                }

                if (!_lockHolders.TryGetValue(resourceName, out string currentHolder) || currentHolder != holderName)
                {
                    STLog.Error($"[{holderName}] 无权释放资源锁：{resourceName}，当前持有者：{currentHolder ?? "未知"}");
                    return;
                }

                Monitor.Exit(lockObj);
                _lockHolders.Remove(resourceName);
                STLog.Debug($"[{holderName}] 成功释放资源锁：{resourceName}");
            }
        }

        /// <summary>
        /// 强制释放所有资源锁（紧急情况使用）
        /// </summary>
        public void ForceReleaseAllLocks()
        {
            lock (_dictLock)
            {
                foreach (var kv in _resourceLocks)
                {
                    // 尝试释放锁，忽略异常
                    try
                    {
                        while (Monitor.IsEntered(kv.Value))
                        {
                            Monitor.Exit(kv.Value);
                        }
                    }
                    catch { }
                }
                _lockHolders.Clear();
                STLog.Warn("已强制释放所有资源锁");
            }
        }

        /// <summary>
        /// 检查资源是否被锁定
        /// </summary>
        public bool IsLocked(string resourceName)
        {
            lock (_dictLock)
            {
                return _lockHolders.ContainsKey(resourceName);
            }
        }
        #endregion
    }
}