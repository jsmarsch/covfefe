using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Covfefe
{
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
}
