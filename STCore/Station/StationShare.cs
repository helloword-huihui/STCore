using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCore.Station
{
    /// <summary>
    /// 全局状态共享类
    /// 所有工位之间、工位与界面之间的交互都通过这里
    /// 替代之前的SystemStatus，和你现有文件名保持一致
    /// </summary>
    public static class StationShare
    {
        #region 系统模式
        /// <summary>
        /// 是否为自动运行模式
        /// </summary>
        public static bool IsAutoMode { get; set; } = false;

        /// <summary>
        /// 是否为手动模式
        /// </summary>
        public static bool IsManualMode { get; set; } = true;

        /// <summary>
        /// 是否为单步模式
        /// </summary>
        public static bool IsSingleStepMode { get; set; } = false;
        #endregion

        #region 全局状态
        /// <summary>
        /// 是否急停
        /// </summary>
        public static bool IsEmergencyStop { get; set; } = false;

        /// <summary>
        /// 是否有全局报警
        /// </summary>
        public static bool IsGlobalAlarm { get; set; } = false;

        /// <summary>
        /// 是否暂停
        /// </summary>
        public static bool IsPaused { get; set; } = false;
        #endregion

        #region 生产统计
        /// <summary>
        /// 总产量
        /// </summary>
        public static int TotalCount { get; set; } = 0;

        /// <summary>
        /// 良品数
        /// </summary>
        public static int OKCount { get; set; } = 0;

        /// <summary>
        /// 不良品数
        /// </summary>
        public static int NGCount { get; set; } = 0;

        /// <summary>
        /// 当前CT时间（毫秒）
        /// </summary>
        public static int CurrentCycleTime { get; set; } = 0;
        #endregion

        #region 工位握手信号（根据你的项目需求添加）
        // 示例：
        // public static bool LoadingComplete { get; set; } = false;
        // public static bool TransferReady { get; set; } = false;
        // public static bool ProcessFinish { get; set; } = false;
        // public static bool UnloadComplete { get; set; } = false;
        #endregion
    }
}
