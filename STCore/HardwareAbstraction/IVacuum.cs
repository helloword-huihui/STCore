using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCore.HardwareAbstraction
{
    /// <summary>
    /// 真空发生器通用接口
    /// </summary>
    public interface IVacuum
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 是否有真空
        /// </summary>
        bool HasVacuum { get; }

        /// <summary>
        /// 开真空
        /// </summary>
        void On();

        /// <summary>
        /// 关真空
        /// </summary>
        void Off();

        /// <summary>
        /// 开吹气
        /// </summary>
        void Open();
        /// <summary>
        /// 关吹气
        /// </summary>
        void Close();
    }
}
