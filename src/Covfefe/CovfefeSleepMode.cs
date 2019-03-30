using System.ComponentModel;

namespace Covfefe
{
    public enum CovfefeSleepMode
    {
        [Description("Normal")]
        Normal,
        [Description("Stay Awake!")]
        StayAwake,
        [Description("Keep Monitors On")]
        MonitorsOn
    }
}