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
    }

    // orchestrates sleep operations
    public class SleepManagementFacade: ISleepManagementFacade
    {
        private readonly IPowerManagementEngine _powerManagementEngine;

        public SleepManagementFacade(IPowerManagementEngine powerManagementEngine)
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
    }
}
