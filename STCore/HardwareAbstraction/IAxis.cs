using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCore.HardwareAbstraction
{
    /// <summary>
    /// 单轴通用接口
    /// </summary>
    public interface IAxis
    {
        /// <summary>
        /// 轴号
        /// </summary>
        int AxisNo { get; }

        /// <summary>
        /// 当前位置（脉冲）
        /// </summary>
        int Position { get; }

        /// <summary>
        /// 当前速度（脉冲/秒）
        /// </summary>
        int Speed { get; }

        /// <summary>
        /// 是否到位
        /// </summary>
        bool IsInPosition { get; }

        /// <summary>
        /// 是否正在运动
        /// </summary>
        bool IsMoving { get; }

        /// <summary>
        /// 是否报警
        /// </summary>
        bool IsAlarm { get; }

        /// <summary>
        /// 绝对运动
        /// </summary>
        /// <param name="position">目标位置（脉冲）</param>
        /// <param name="speed">速度（脉冲/秒）</param>
        void MoveAbs(double position, double speed);

        /// <summary>
        /// 相对运动
        /// </summary>
        /// <param name="distance">相对距离（脉冲）</param>
        /// <param name="speed">速度（脉冲/秒）</param>
        void MoveRel(double distance, double speed);

        /// <summary>
        /// 回零
        /// </summary>
        /// <param name="speed">回零速度（脉冲/秒）</param>
        void Home(double speed);

        /// <summary>
        /// 停止运动
        /// </summary>
        void Stop();

        /// <summary>
        /// 清除报警
        /// </summary>
        void ClearAlarm();

        /// <summary>
        /// 设置当前位置为原点
        /// </summary>
        void SetHome();
    }
}
