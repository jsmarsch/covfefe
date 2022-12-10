using System;

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
