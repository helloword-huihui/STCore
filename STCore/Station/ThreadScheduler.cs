using STCore.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STCore.Station
{
    /// <summary>
    /// 线程调度器
    /// 每个工位一个独立线程，精确50ms周期运行
    /// </summary>
    public class ThreadScheduler
    {
        #region 单例模式
        public static ThreadScheduler Instance { get; } = new ThreadScheduler();
        private ThreadScheduler() { }
        #endregion

        #region 私有字段
        private readonly List<Thread> _workerThreads = new List<Thread>();
        private bool _isRunning;
        private readonly object _lockObj = new object();
        private const int CycleTimeMs = 50; // 标准工控周期50ms
        private readonly ManualResetEvent _pauseEvent = new ManualResetEvent(true); // 默认允许运行
        #endregion

        #region 公共属性
        /// <summary>
        /// 调度器是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;
        #endregion

        #region 公共方法
        /// <summary>
        /// 初始化调度器
        /// </summary>
        public void Init()
        {
            _isRunning = false;
            _workerThreads.Clear();
            ST_Log.Info("线程调度器初始化完成");
        }

        /// <summary>
        /// 启动所有工位线程
        /// 有几个注册的工位，就启动几个线程
        /// </summary>
        public void Start()
        {
            lock (_lockObj)
            {
                if (_isRunning)
                {
                    ST_Log.Warn("线程调度器已经在运行了");
                    return;
                }

                int stationCount = StationManager.Instance.StationCount;
                if (stationCount == 0)
                {
                    ST_Log.Warn("没有注册任何工位，无需启动线程");
                    return;
                }

                ST_Log.Info($"开始启动{stationCount}个工位线程...");

                // 为每个工位创建一个独立的线程
                for (int i = 0; i < stationCount; i++)
                {
                    int stationIndex = i;
                    var station = StationManager.Instance.AllStations[stationIndex];

                    Thread thread = new Thread(() => WorkerThreadProc(station))
                    {
                        Name = $"工位线程-{station.StationName}",
                        IsBackground = true, // 后台线程，程序退出时自动结束
                        Priority = ThreadPriority.AboveNormal // 工控线程优先级高于普通线程
                    };

                    _workerThreads.Add(thread);
                    thread.Start();
                }

                _isRunning = true;
                ST_Log.Info("所有工位线程启动完成");
            }
        }

        /// <summary>
        /// 停止所有工位线程
        /// </summary>
        public void Stop()
        {
            lock (_lockObj)
            {
                if (!_isRunning)
                {
                    ST_Log.Warn("线程调度器没有在运行");
                    return;
                }

                ST_Log.Info("开始停止所有工位线程...");

                _isRunning = false;

                // 等待所有线程结束
                foreach (var thread in _workerThreads)
                {
                    if (thread.IsAlive&& !thread.Join(1000))
                    {
                         // 只有在万不得已时才强制终止
                        ST_Log.Warn($"线程[{thread.Name}]强制终止");
                        thread.Abort();
                    }
                }

                _workerThreads.Clear();
                ST_Log.Info("所有工位线程已停止");
            }
        }
          /// <summary>
         /// 暂停所有工位线程
         /// </summary>
        public void Pause()
        {
            lock (_lockObj)
            {
                if (!_isRunning || StationShare.IsPaused)
                {
                    ST_Log.Warn("无法暂停：调度器未运行或已暂停");
                    return;
                }

                StationShare.IsPaused = true;
                _pauseEvent.Reset(); // 阻塞所有线程
                ST_Log.Info("所有工位线程已暂停");
            }
        }

        /// <summary>
        /// 继续所有工位线程
        /// </summary>
        public void Resume()
        {
            lock (_lockObj)
            {
                if (!_isRunning || !StationShare.IsPaused)
                {
                    ST_Log.Warn("无法继续：调度器未运行或未暂停");
                    return;
                }

                StationShare.IsPaused = false;
                _pauseEvent.Set(); // 唤醒所有线程
                ST_Log.Info("所有工位线程已继续运行");
            }
        }

        /// <summary>
        /// 单步执行一次所有工位
        /// </summary>
        public void StepOnce()
        {
            if (_isRunning)
            {
                ST_Log.Warn("自动模式下不能单步执行");
                return;
            }

            ST_Log.Info("单步执行所有工位");
            StationManager.Instance.RunAllStations();
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 工位线程工作函数
        /// </summary>
        private void WorkerThreadProc(BaseStation station)
        {
            ST_Log.Info($"工位[{station.StationName}]线程已启动");

            Stopwatch stopwatch = new Stopwatch();

            while (_isRunning)
            {  
                // 暂停等待：暂停时线程会在这里阻塞，不消耗CPU
                _pauseEvent.WaitOne();

                // 再次检查运行状态，防止停止时还在执行
                if (!_isRunning) break;
                stopwatch.Restart();

                try
                {
                    // 运行工位逻辑
                    station.Run();
                }
                catch (Exception ex)
                {
                    ST_Log.Error($"工位[{station.StationName}]线程异常：{ex.Message}", ex);
                }
                finally
                {
                    // 精确延时，保证50ms周期
                    int elapsedMs = (int)stopwatch.ElapsedMilliseconds;
                    int sleepMs = CycleTimeMs - elapsedMs;

                    if (sleepMs > 0)
                    {
                        Thread.Sleep(sleepMs);
                    }
                    else
                    {
                        // 周期超时，记录警告
                        ST_Log.Warn($"工位[{station.StationName}]周期超时，耗时{elapsedMs}ms");
                    }
                }
            }

            ST_Log.Info($"工位[{station.StationName}]线程已停止");
        }
        #endregion
    }
}
