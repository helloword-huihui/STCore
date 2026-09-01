using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCore.HardwareAbstraction
{
    /// <summary>
    /// IO通用接口
    /// </summary>
    public interface IIO
    {
        /// <summary>
        /// 读取单个输入点
        /// </summary>
        /// <param name="ioNo">IO号（0开始）</param>
        /// <returns>是否导通</returns>
        bool ReadInput(int ioNo);

        /// <summary>
        /// 写入单个输出点
        /// </summary>
        /// <param name="ioNo">IO号（0开始）</param>
        /// <param name="value">值</param>
        void WriteOutput(int ioNo, bool value);

        /// <summary>
        /// 批量读取输入
        /// </summary>
        /// <param name="startNo">起始IO号</param>
        /// <param name="count">数量</param>
        /// <returns>值数组</returns>
        bool[] ReadInputs(int startNo, int count);

        /// <summary>
        /// 批量写入输出
        /// </summary>
        /// <param name="startNo">起始IO号</param>
        /// <param name="values">值数组</param>
        void WriteOutputs(int startNo, bool[] values);

        /// <summary>
        /// 复位所有输出
        /// </summary>
        void ResetAllOutputs();
    }
}
