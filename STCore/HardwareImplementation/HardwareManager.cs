using STCore.HardwareAbstraction;
using STCore.Log;
using STCore.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCore.HardwareImplementation
{
    /// <summary>
    /// 硬件管理器
    /// 统一管理所有硬件，根据配置自动创建对应实例
    /// </summary>
    public class HardwareManager
    {
        #region 单例模式
        public static HardwareManager Instance { get; } = new HardwareManager();
        private HardwareManager() { }
        #endregion

        #region 公共属性
        /// <summary>
        /// 当前使用的运动卡
        /// </summary>
        public IMotionCard MotionCard { get; private set; }

        /// <summary>
        /// IO控制器
        /// </summary>
        public IIO IO => MotionCard?.GetIO();
        #endregion

        #region 公共方法
        /// <summary>
        /// 初始化所有硬件
        /// 根据配置文件自动选择运动卡类型
        /// </summary>
        /// <returns>是否成功</returns>
        public bool Init()
        {
            try
            {
                STLog.Info("开始初始化硬件...");

                // 从配置文件读取硬件类型和卡号
                string motionCardType = ParameterManager.Instance.GetValue("MotionCard.Type", "LeiSai");
                int cardNo = ParameterManager.Instance.GetValue("MotionCard.CardNo", 0);
                int axisCount = ParameterManager.Instance.GetValue("MotionCard.AxisCount", 8);

                STLog.Info($"运动卡类型：{motionCardType}，卡号：{cardNo}，轴数：{axisCount}");

                // 根据配置创建对应的运动卡实例
                switch (motionCardType.ToLower())
                {
                    case "leisai":
                        MotionCard = new LeiSaiMotionCard(axisCount);
                        break;
                    case "googol":
                        MotionCard = new GoogolMotionCard(axisCount);
                        break;
                    case "lingchen":
                        MotionCard = new LingChenMotionCard(axisCount);
                        break;
                    default:
                        throw new NotSupportedException($"不支持的运动卡类型：{motionCardType}");
                }

                // 初始化运动卡
                if (!MotionCard.Init(cardNo))
                {
                    STLog.Error("运动卡初始化失败");
                    return false;
                }

                STLog.Info("硬件初始化完成");
                return true;
            }
            catch (Exception ex)
            {
                STLog.Fatal($"硬件初始化失败：{ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 关闭所有硬件
        /// </summary>
        public void Close()
        {
            try
            {
                STLog.Info("开始关闭硬件...");

                MotionCard?.StopAllAxes();
                MotionCard?.GetIO()?.ResetAllOutputs();
                MotionCard?.Close();

                STLog.Info("硬件已关闭");
            }
            catch (Exception ex)
            {
                STLog.Error($"关闭硬件失败：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 紧急停止
        /// </summary>
        public void EmergencyStop()
        {
            STLog.Fatal("触发硬件紧急停止");
            MotionCard?.EmergencyStop();
            MotionCard?.GetIO()?.ResetAllOutputs();
        }

        /// <summary>
        /// 停止所有轴
        /// </summary>
        public void StopAllAxes()
        {
            MotionCard?.StopAllAxes();
        }

        /// <summary>
        /// 复位所有危险输出
        /// </summary>
        public void ResetAllDangerousOutputs()
        {
            MotionCard?.GetIO()?.ResetAllOutputs();
        }

        /// <summary>
        /// 获取指定轴
        /// </summary>
        public IAxis GetAxis(ushort axisNo)
        {
            return MotionCard?.GetAxis(axisNo);
        }
        #endregion
    }
}
