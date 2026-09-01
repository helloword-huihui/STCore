using STCore.HardwareAbstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCore.HardwareImplementation
{
    public class GoogolMotionCard : IMotionCard
    {
        public void Close()
        {
            throw new NotImplementedException();
        }

        public void EmergencyStop()
        {
            throw new NotImplementedException();
        }

        public IAxis GetAxis(int axisNo)
        {
            throw new NotImplementedException();
        }

        public IIO GetIO()
        {
            throw new NotImplementedException();
        }

        public bool Init(int cardNo)
        {
            throw new NotImplementedException();
        }

        public void StopAllAxes()
        {
            throw new NotImplementedException();
        }
    }
}
