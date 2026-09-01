
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCore.Station
{
    /// <summary>
    /// 所有工位的基类
    /// 外部工位只需要继承此类，重写AutoRun()方法即可
    /// </summary>
    public abstract class BaseStation
    {
        #region 公共属性（外部可读取）
        /// <summary>
        /// 当前步号
        /// </summary>
        public int StepNo { get; private set; }

        /// <summary>
        /// 工位名称
        /// </summary>
        public string StationName { get; set; } = "未命名工位";

        /// <summary>
        /// 工位是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 工位是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }
        #endregion

        #region 保护字段（子类可访问）
        /// <summary>
        /// 预跳转步号（本周期结束后生效）
        /// </summary>
        protected int NextStep;

        /// <summary>
        /// 单步超时计时器
        /// </summary>
        protected DateTime TimerStart;
        #endregion

        #region 构造函数
        protected BaseStation()
        {
            // 默认初始步号为10
            StepNo = 10;
            NextStep = 10;
        }
        #endregion

        #region 核心运行方法（框架内部调用，外部不可见）
        /// <summary>
        /// 工位运行入口
        /// 由线程调度器每50ms调用一次
        /// </summary>
        internal void Run()
        {
            // 全局安全检查：急停/报警/暂停→直接复位
            if (StationShare.IsEmergencyStop || StationShare.IsGlobalAlarm || StationShare.IsPaused)
            {
                Reset();
                return;
            }

            // 非自动模式或工位未启用→不运行
            if (!StationShare.IsAutoMode || !IsEnabled)
            {
                return;
            }

            IsRunning = true;

            try
            {
                // 执行子类的工艺逻辑
                AutoRun();

                // 统一跳步：本周期结束后更新当前步号
                StepNo = NextStep;
            }
            catch (Exception ex)
            {
                // 工位异常自动触发报警
                AddAlarm($"工位[{StationName}]运行异常：{ex.Message}");
                Reset();
            }
            finally
            {
                IsRunning = false;
            }
        }
        #endregion

        #region 抽象方法（子类必须重写）
        /// <summary>
        /// 工艺逻辑方法
        /// 外部工位只需要重写这个方法
        /// </summary>
        protected abstract void AutoRun();
        #endregion

        #region 保护工具方法（子类可调用）
        /// <summary>
        /// 启动单步超时计时器
        /// </summary>
        protected void StartTimer()
        {
            TimerStart = DateTime.Now;
        }

        /// <summary>
        /// 判断当前步骤是否超时
        /// </summary>
        /// <param name="timeoutMs">超时时间（毫秒）</param>
        /// <returns>是否超时</returns>
        protected bool IsTimeout(int timeoutMs)
        {
            return (DateTime.Now - TimerStart).TotalMilliseconds > timeoutMs;
        }

        /// <summary>
        /// 设置下一步号
        /// </summary>
        /// <param name="step">目标步号</param>
        protected void SetStep(int step)
        {
            NextStep = step;
        }

        /// <summary>
        /// 触发报警
        /// </summary>
        /// <param name="message">报警信息</param>
        protected void AddAlarm(string message)
        {
            AlarmSystem.Instance.AddAlarm($"{StationName}：{message}");
        }

        /// <summary>
        /// 复位工位到初始状态
        /// </summary>
        public virtual void Reset()
        {
            StepNo = 10;
            NextStep = 10;
            IsRunning = false;
        }
        #endregion
    }

}
