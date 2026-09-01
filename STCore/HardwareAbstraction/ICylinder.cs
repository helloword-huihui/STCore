using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCore.HardwareAbstraction
{
    /// <summary>
    /// 气缸通用接口
    /// </summary>
    public interface ICylinder
    {
        /// <summary>
        /// 气缸名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 是否伸出到位
        /// </summary>
        bool IsExtended { get; }

        /// <summary>
        /// 是否缩回到位
        /// </summary>
        bool IsRetracted { get; }

        /// <summary>
        /// 伸出
        /// </summary>
        void Extend();

        /// <summary>
        /// 缩回
        /// </summary>
        void Retract();
    }
}
