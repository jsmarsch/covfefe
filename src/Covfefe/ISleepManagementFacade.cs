using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Covfefe
{
    public interface ISleepManagementFacade
    {
        void KeepMonitorsAwake();
        void DisableSleep();
        void ClearSleepOverrides();
        void DelaySleepOnBatteryStateChange(TimeSpan delay);
        void DelaySleepOnLidClose(TimeSpan delay);
    }
}
