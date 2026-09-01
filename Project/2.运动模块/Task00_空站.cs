using STCore.HardwareImplementation;
using STCore.Parameter;
using STCore.Station;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project
{
    public class Task00_空站 : BaseStation
    {
        public Task00_空站()
        {
            StationName = "上料工位";
        }
        protected override void AutoRun()
        {
            switch (StepNo)
            {
                case 0:
                    HardwareShare.ReadDI(1);
                    HardwareShare.RelMotion(1, 22, 1);

                    // 获取设备名称
                    string deviceName = ParameterManager.Instance.DeviceInfo.DeviceName;

                    // 获取X轴速度
                    double xSpeed = ParameterManager.Instance.GetAxisParam(0).Speed;

                    // 获取夹紧延时
                    int clampDelay = ParameterManager.Instance.GetProgramParam<int>("夹紧延时");

                    // 获取自动回零开关
                    bool autoHome = ParameterManager.Instance.GetSwitchParam("自动回零");

                    // 获取X轴补偿
                    double xComp = ParameterManager.Instance.GetCompensationParam("X轴机械补偿");

                    // 获取报警代码信息
                    var alarm = ParameterManager.Instance.GetAlarmCode(1001);
                    MessageBox.Show(alarm.Solution);

                    // 设置参数
                    ParameterManager.Instance.SetProgramParam("夹紧延时", 1200);
                    ParameterManager.Instance.SetSwitchParam("报警蜂鸣", false);
                    ParameterManager.Instance.SetCompensationParam("X轴机械补偿", 0.5);



                    NextStep = 20;
                    break;
            }
        }
    }
}
