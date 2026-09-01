using STCore.HardwareAbstraction;
using STCore.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace STCore.HardwareImplementation
{
    public static class HardwareShare
    {
        /// <summary>
        /// 硬件操作全局锁（所有运动卡DLL都不是线程安全的，必须加锁）
        /// </summary>
        private static readonly object _hardwareLock = new object();

        /// <summary>
        /// 模拟模式开关（true=不连接硬件，所有操作返回成功）
        /// </summary>
        public static bool IsSimulationMode { get; set; } = false;


        /// <summary>
        /// IO
        /// </summary>
        public static IIO IO => HardwareManager.Instance.IO;



        #region IO
        public static bool ReadDI(int index)
        {
            return IO.ReadInput(index);
        }
        public static void WriteDo(int index, bool value)
        {
            IO.WriteOutput(index, value);
        }
        #endregion


        #region 运动等待方法
        /// <summary>
        /// 绝对位置运动（毫米单位）
        /// </summary>
        /// <param name="position">目标位置（mm）</param>
        /// <param name="vel">运动速度（mm/s）</param>
        public static void AbsMotion(ushort AxisId, double position, double vel)
        {
            if (IsSimulationMode)
            {
                return;
            }
            lock (_hardwareLock)
            {
                HardwareManager.Instance.GetAxis(AxisId).MoveAbs(position, vel);
            }
        }
        /// <summary>
        /// 相对位置运动（毫米单位）
        /// </summary>
        /// <param name="distance">相对距离（mm）</param>
        /// <param name="vel">运动速度（mm/s）</param>
        public static void RelMotion(ushort AxisId, double distance, double vel)
        {
            if (IsSimulationMode)
            {
                return;
            }

            lock (_hardwareLock)
            {
                HardwareManager.Instance.GetAxis(AxisId).MoveRel(distance, vel);
            }
        }
        public static void Home(double vel = 50)
        {
      
        }

      
        /// <summary>
        /// 绝对运动并等待到位
        /// </summary>
        /// <param name="position">目标位置（mm）</param>
        /// <param name="vel">运动速度（mm/s）</param>
        /// <param name="timeoutMs">超时时间（毫秒）</param>
        /// <returns>是否到位成功</returns>
        public bool AbsMotionAndWait(double position, double vel, int timeoutMs = 10000)
        {
            AbsMotion(position, vel);
            return WaitForMoveDone(timeoutMs);
        }

        /// <summary>
        /// 相对运动并等待到位
        /// </summary>
        public bool RelMotionAndWait(double distance, double vel, int timeoutMs = 10000)
        {
            RelMotion(distance, vel);
            return WaitForMoveDone(timeoutMs);
        }

        /// <summary>
        /// 回零并等待完成
        /// </summary>
        public bool HomeAndWait(double vel = 50, int timeoutMs = 30000)
        {
            Home(vel);
            return WaitForMoveDone(timeoutMs);
        }

        /// <summary>
        /// 等待运动完成
        /// </summary>
        public bool WaitForMoveDone(int timeoutMs = 10000)
        {
            if (IsSimulationMode)
            {
                Thread.Sleep(500);
                return true;
            }

            DateTime start = DateTime.Now;
            while (!AxisMoveDone)
            {
                if ((DateTime.Now - start).TotalMilliseconds > timeoutMs)
                {
                    STLog.Error($"{Name}运动超时，当前位置：{PosMm:F3}mm");
                    StopMove();
                    return false;
                }
                Thread.Sleep(10);
            }
            return true;
        }
        #endregion

        #region 状态查询属性
        /// <summary>
        /// 运动是否完成
        /// </summary>
        public bool AxisMoveDone
        {
            get
            {
                if (IsSimulationMode) return true;
                lock (_hardwareLock)
                {
                    return MotionDll.AxisMoveDone(AxisNo);
                }
            }
        }

        /// <summary>
        /// 当前位置（毫米）
        /// </summary>
        public double GetPosMm
        {
            get
            {
                if (IsSimulationMode) return 0;
                lock (_hardwareLock)
                {
                    return MotionDll.GetPosMm(AxisNo);
                }
            }
        }

        /// <summary>
        /// 当前速度（毫米/秒）
        /// </summary>
        public double GetVelMm
        {
            get
            {
                if (IsSimulationMode) return 0;
                lock (_hardwareLock)
                {
                    return MotionDll.GetVelMm(AxisNo);
                }
            }
        }

        /// <summary>
        /// 是否有报警
        /// </summary>
        public bool IsAlarm
        {
            get
            {
                if (IsSimulationMode) return false;
                lock (_hardwareLock)
                {
                    return MotionDll.AxisAlarm(AxisNo) != 0;
                }
            }
        }

        /// <summary>
        /// 正限位
        /// </summary>
        public bool LimitF
        {
            get
            {
                if (IsSimulationMode) return false;
                lock (_hardwareLock)
                {
                    return MotionDll.LimitF(AxisNo) != 0;
                }
            }
        }

        /// <summary>
        /// 负限位
        /// </summary>
        public bool LimitZ
        {
            get
            {
                if (IsSimulationMode) return false;
                lock (_hardwareLock)
                {
                    return MotionDll.LimitZ(AxisNo) != 0;
                }
            }
        }

        /// <summary>
        /// 原点信号
        /// </summary>
        public bool HomeBit
        {
            get
            {
                if (IsSimulationMode) return false;
                lock (_hardwareLock)
                {
                    return MotionDll.HomeBit(AxisNo) != 0;
                }
            }
        }

        /// <summary>
        /// 伺服是否使能
        /// </summary>
        public bool IsServoOn
        {
            get
            {
                if (IsSimulationMode) return true;
                lock (_hardwareLock)
                {
                    return MotionDll.GetServoOnSts(AxisNo) != 0;
                }
            }
        }
        #endregion

        #region 伺服控制方法
        /// <summary>
        /// 伺服使能
        /// </summary>
        public void ServoOn()
        {
            if (IsSimulationMode)
            {
                STLog.Debug($"[模拟] {Name}伺服使能");
                return;
            }

            lock (_hardwareLock)
            {
                STLog.Debug($"{Name}伺服使能");
                MotionDll.ServoOn(AxisNo);
            }
        }

        /// <summary>
        /// 伺服失能
        /// </summary>
        public void ServoOff()
        {
            if (IsSimulationMode)
            {
                STLog.Debug($"[模拟] {Name}伺服失能");
                return;
            }

            lock (_hardwareLock)
            {
                STLog.Debug($"{Name}伺服失能");
                MotionDll.ServoOff(AxisNo);
            }
        }

        /// <summary>
        /// 清除报警
        /// </summary>
        public void ClrSts()
        {
            if (IsSimulationMode)
            {
                STLog.Debug($"[模拟] {Name}清除报警");
                return;
            }

            lock (_hardwareLock)
            {
                STLog.Debug($"{Name}清除报警");
                MotionDll.ClrSts(AxisNo);
            }
        }

        /// <summary>
        /// 设置当前位置为原点
        /// </summary>
        public void SetHome()
        {
            if (IsSimulationMode)
            {
                STLog.Debug($"[模拟] {Name}设置当前位置为原点");
                return;
            }

            lock (_hardwareLock)
            {
                STLog.Debug($"{Name}设置当前位置为原点");
                MotionDll.SetEncPos(AxisNo, 0);
            }
        }
        #endregion

        #region 参数设置方法
        /// <summary>
        /// 设置速度
        /// </summary>
        public void SetVel(double vel)
        {
            if (IsSimulationMode)
            {
                STLog.Debug($"[模拟] {Name}设置速度为{vel:F3}mm/s");
                return;
            }

            lock (_hardwareLock)
            {
                STLog.Debug($"{Name}设置速度为{vel:F3}mm/s");
                MotionDll.SetVel(AxisNo, vel);
            }
        }

        /// <summary>
        /// 设置加减速度
        /// </summary>
        public void SetAcc(double acc, double dec = -1)
        {
            if (IsSimulationMode)
            {
                STLog.Debug($"[模拟] {Name}设置加速度为{acc:F3}mm/s²");
                return;
            }

            lock (_hardwareLock)
            {
                STLog.Debug($"{Name}设置加速度为{acc:F3}mm/s²");
                MotionDll.SetAcc(AxisNo, acc, dec);
            }
        }
        #endregion
    }
}
}
