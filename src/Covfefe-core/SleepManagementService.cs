using Covfefe.Contracts;

namespace Covfefe
{
    public class SleepManagementService: ISleepManagementService
    {
        private readonly IPowerManagementEngine _powerManagementEngine;

        public SleepManagementService(IPowerManagementEngine powerManagementEngine)
        {
            _powerManagementEngine = powerManagementEngine;
        }

        public void KeepMonitorsAwake()
        {
            _powerManagementEngine.KeepMonitorsOn();
        }

        public void DisableSleep()
        {
            _powerManagementEngine.PreventSleep();
        }

        public void ClearSleepOverrides()
        {
            _powerManagementEngine.ClearPowerManagementOverrides();
        }

        public void DelaySleepOnBatteryStateChange(TimeSpan delay)
        {
            throw new NotImplementedException();
        }

        public void DelaySleepOnLidClose(TimeSpan delay)
        {
            throw new NotImplementedException();
        }
    }
}