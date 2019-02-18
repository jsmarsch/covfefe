using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Covfefe
{
    public interface IPowerManagementEngine
    {
        void PreventSleep();
        void ClearPowerManagementOverrides();
        void KeepMonitorsOn();
    }

    public static class WindowsPowerManagementApi
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [Flags]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

    }
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
