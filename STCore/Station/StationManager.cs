using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCore.Station
{
    /// <summary>
    /// 工位管理器
    /// 负责注册、管理所有工位
    /// </summary>
    public class StationManager
    {
        #region 单例模式
        public static StationManager Instance { get; } = new StationManager();
        private StationManager() { }
        #endregion

        #region 私有字段
        private readonly List<BaseStation> _stations = new List<BaseStation>();
        #endregion

        #region 公共属性
        /// <summary>
        /// 所有注册的工位列表
        /// </summary>
        public IReadOnlyList<BaseStation> AllStations => _stations.AsReadOnly();

        /// <summary>
        /// 注册的工位数量
        /// </summary>
        public int StationCount => _stations.Count;
        #endregion

        #region 公共方法
        /// <summary>
        /// 注册一个工位
        /// </summary>
        /// <param name="station">工位实例</param>
        public void RegisterStation(BaseStation station)
        {
            if (station == null)
                throw new ArgumentNullException(nameof(station));

            if (_stations.Any(s => s.StationName == station.StationName))
                throw new InvalidOperationException($"工位名称[{station.StationName}]已存在");

            _stations.Add(station);
        }

        /// <summary>
        /// 复位所有工位
        /// </summary>
        public void ResetAllStations()
        {
            foreach (var station in _stations)
            {
                station.Reset();
            }
        }

        /// <summary>
        /// 根据名称获取工位
        /// </summary>
        /// <param name="stationName">工位名称</param>
        /// <returns>工位实例</returns>
        public BaseStation GetStationByName(string stationName)
        {
            return _stations.FirstOrDefault(s => s.StationName == stationName);
        }

        /// <summary>
        /// 运行所有启用的工位
        /// </summary>
        internal void RunAllStations()
        {
            foreach (var station in _stations.Where(s => s.IsEnabled))
            {
                station.Run();
            }
        }
        #endregion
    }
}
