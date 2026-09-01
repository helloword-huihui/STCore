using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCore.HardwareAbstraction
{
    /// <summary>
    /// 运动卡通用接口
    /// 所有品牌运动卡都必须实现此接口
    /// </summary>
    public interface IMotionCard
    {
        /// <summary>
        /// 初始化运动卡
        /// </summary>
        /// <param name="cardNo">卡号</param>
        /// <returns>是否成功</returns>
        bool Init(int cardNo);

        /// <summary>
        /// 关闭运动卡
        /// </summary>
        void Close();

        /// <summary>
        /// 急停所有轴
        /// </summary>
        void EmergencyStop();

        /// <summary>
        /// 停止所有轴
        /// </summary>
        void StopAllAxes();

        /// <summary>
        /// 获取指定轴
        /// </summary>
        /// <param name="axisNo">轴号（0开始）</param>
        /// <returns>轴实例</returns>
        IAxis GetAxis(int axisNo);

        /// <summary>
        /// 获取IO控制器
        /// </summary>
        /// <returns>IO实例</returns>
        IIO GetIO();
    }
}
