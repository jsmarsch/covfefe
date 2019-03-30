namespace Covfefe
{
    public class CovfefeSettings
    {
        public bool StartAtLogin { get; set; }
        public bool ShowBalloonTips { get; set; }
        public bool ShowReminder { get; set; }
        public int ReminderTimeoutMinutes { get; set; }
        public CovfefeSleepMode DefaultSleepMode { get; set; }
    }
}