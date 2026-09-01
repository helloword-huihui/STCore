using STCore.HardwareAbstraction;
using STCore.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCore.HardwareImplementation
{
    /// <summary>
    /// 雷赛运动卡实现
    /// </summary>
    public class LeiSaiMotionCard : IMotionCard
    {
        private int _cardNo;
        private readonly LeiSaiAxis[] _axes;
        private readonly LeiSaiIO _io;

        public LeiSaiMotionCard(int axisCount)
        {
            _axes = new LeiSaiAxis[axisCount];
            _io = new LeiSaiIO();
        }

        public bool Init(int cardNo)
        {
            _cardNo = cardNo;
            STLog.Info($"初始化雷赛运动卡，卡号：{cardNo}");

            // 替换为雷赛DLL的初始化函数
            // int result = LTDMC.dmc_open_card(cardNo);
            int result = 0; // 模拟成功

            if (result != 0)
            {
                STLog.Error($"雷赛运动卡初始化失败，错误码：{result}");
                return false;
            }

            // 初始化所有轴
            for (int i = 0; i < _axes.Length; i++)
            {
                _axes[i] = new LeiSaiAxis(cardNo, i);
            }

            _io.Init(cardNo);
            STLog.Info("雷赛运动卡初始化成功");
            return true;
        }

        public void Close()
        {
            STLog.Info("关闭雷赛运动卡");
            // LTDMC.dmc_close_card(_cardNo);
        }

        public void EmergencyStop()
        {
            STLog.Warn("雷赛运动卡急停");
            // LTDMC.dmc_emg_stop(_cardNo);
        }

        public void StopAllAxes()
        {
            STLog.Info("停止雷赛运动卡所有轴");
            // LTDMC.dmc_stop_all(_cardNo);
        }

        public IAxis GetAxis(int axisNo)
        {
            if (axisNo < 0 || axisNo >= _axes.Length)
                throw new ArgumentOutOfRangeException(nameof(axisNo));

            return _axes[axisNo];
        }

        public IIO GetIO()
        {
            return _io;
        }
    }

    /// <summary>
    /// 雷赛单轴实现
    /// </summary>
    internal class LeiSaiAxis : IAxis
    {
        private readonly int _cardNo;
        private readonly int _axisNo;

        public int AxisNo => _axisNo;

        public int Position
        {
            get
            {
                // return LTDMC.dmc_get_position(_cardNo, _axisNo);
                return 0;
            }
        }

        public int Speed
        {
            get
            {
                // return LTDMC.dmc_get_speed(_cardNo, _axisNo);
                return 0;
            }
        }

        public bool IsInPosition
        {
            get
            {
                // return LTDMC.dmc_check_done(_cardNo, _axisNo) == 1;
                return true;
            }
        }

        public bool IsMoving => !IsInPosition;

        public bool IsAlarm
        {
            get
            {
                // return LTDMC.dmc_get_axis_status(_cardNo, _axisNo) != 0;
                return false;
            }
        }

        public LeiSaiAxis(int cardNo, int axisNo)
        {
            _cardNo = cardNo;
            _axisNo = axisNo;
        }

        public void MoveAbs(double position, double speed)
        {
            STLog.Debug($"轴{_axisNo}绝对运动到{position}，速度{speed}");
            // LTDMC.dmc_pmove(_cardNo, _axisNo, position, speed);
        }

        public void MoveRel(double distance, double speed)
        {
            STLog.Debug($"轴{_axisNo}相对运动{distance}，速度{speed}");
            // LTDMC.dmc_rmove(_cardNo, _axisNo, distance, speed);
        }

        public void Home(double speed)
        {
            STLog.Debug($"轴{_axisNo}回零，速度{speed}");
            // LTDMC.dmc_home(_cardNo, _axisNo, speed);
        }

        public void Stop()
        {
            STLog.Debug($"轴{_axisNo}停止");
            // LTDMC.dmc_stop(_cardNo, _axisNo);
        }

        public void ClearAlarm()
        {
            STLog.Debug($"轴{_axisNo}清除报警");
            // LTDMC.dmc_clear_alarm(_cardNo, _axisNo);
        }

        public void SetHome()
        {
            STLog.Debug($"轴{_axisNo}设置当前位置为原点");
            // LTDMC.dmc_set_position(_cardNo, _axisNo, 0);
        }
    }

    /// <summary>
    /// 雷赛IO实现
    /// </summary>
    internal class LeiSaiIO : IIO
    {
        private int _cardNo;

        public void Init(int cardNo)
        {
            _cardNo = cardNo;
        }

        public bool ReadInput(int ioNo)
        {
            // return LTDMC.dmc_read_inbit(_cardNo, ioNo) == 1;
            return false;
        }

        public void WriteOutput(int ioNo, bool value)
        {
            STLog.Debug($"输出{ioNo}设置为{value}");
            // LTDMC.dmc_write_outbit(_cardNo, ioNo, value ? 1 : 0);
        }

        public bool[] ReadInputs(int startNo, int count)
        {
            bool[] values = new bool[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = ReadInput(startNo + i);
            }
            return values;
        }

        public void WriteOutputs(int startNo, bool[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                WriteOutput(startNo + i, values[i]);
            }
        }

        public void ResetAllOutputs()
        {
            STLog.Info("复位所有输出");
            // 实现复位所有输出的逻辑
        }
    }
}
