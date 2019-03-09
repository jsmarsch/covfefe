using System.ComponentModel;

namespace Covfefe
{
    // encapsulates API volatility related to sleep operations
    public class Windows7PowerManagementEngine: IPowerManagementEngine
    {

        public void PreventSleep()
        {
            var result = WindowsPowerManagementApi.SetThreadExecutionState(
                WindowsPowerManagementApi.EXECUTION_STATE.ES_SYSTEM_REQUIRED
                | WindowsPowerManagementApi.EXECUTION_STATE.ES_CONTINUOUS);
            if (result == 0)
                throw new Win32Exception();
        }

        public void KeepMonitorsOn()
        {
            var result = WindowsPowerManagementApi.SetThreadExecutionState(
                WindowsPowerManagementApi.EXECUTION_STATE.ES_DISPLAY_REQUIRED |
                WindowsPowerManagementApi.EXECUTION_STATE.ES_CONTINUOUS);
            if (result == 0)
                throw new Win32Exception();
        }

        public void ClearPowerManagementOverrides()
        {
            var result =
                WindowsPowerManagementApi.SetThreadExecutionState(WindowsPowerManagementApi.EXECUTION_STATE.ES_CONTINUOUS);
            if (result == 0)
                throw new Win32Exception();
        }

    }
}