using Covfefe.ApiImports;
using Covfefe.Contracts;
using Covfefe.Models;

namespace Covfefe
{
    public partial class TrayForm : Form
    {
        private readonly ISleepManagementService _sleepManagementService;
        private bool _isInitialLoad = true;
        private readonly ISettingsService _settingsService;
        private CovfefeSettings _settings;
        private CovfefeSleepMode _sleepMode;
        private bool _lastBalloonWasTheReminder;

        private const string BalloonText_KeepMonitorsOn =
            """Your Monitors will remain powered on, and your computer will not sleep.  To exit this mode, select "Normal Operation" in the context menu, or exit Covfefe.""";

        private const string BalloonText_Normal =
            """Your computer will sleep and wake normally, as configured in your power management settings.  Use the context menu to enable "Stay Awake" or "Monitors On" Mode.""";

        private const string BalloonText_SleepReminder =
            """It's been a while since you have disabled sleep.  Did you forget?  You can click to here to return to Normal Mode.""";

        private const string BalloonText_StayAwake =
            """Your computer will not sleep.  To exit this mode, select "Normal Operation" in the context menu, or exit Covfefe.""";

        private const string BalloonTitle_KeepMonitorsOn = "Keep Monitors On";

        private const string BalloonTitle_Normal = "Normal Operation";

        private const string BalloonTitle_SleepReminder = "Re-enable Sleep?";

        private const string BalloonTitle_StayAwake = "Stay Awake!";

        public TrayForm(ISleepManagementService sleepManagementService, ISettingsService settingsService)
        {
            _sleepManagementService = sleepManagementService;
            _settingsService = settingsService;
            InitializeComponent();

            // NotifyIcon raises BalloonTipClicked for whichever balloon is on screen, whatever
            // showed it, so the handler acts only when _lastBalloonWasTheReminder says the
            // reminder is the balloon being clicked.
            covfefeNotifyIcon.BalloonTipClicked += SleepReminderBalloonTip_Click;

            _settings = _settingsService.GetSettings();
            ConfigureSettingsControls(_settings);
            SetSleepMode(_settings.DefaultSleepMode);
        }

        private void ConfigureSettingsControls(CovfefeSettings settings)
        {
            startOnLoginCheckbox.DataBindings.Add(nameof(startOnLoginCheckbox.Checked), settings, nameof(settings.StartAtLogin));
            showBalloonTipsCheckbox.DataBindings.Add(nameof(showBalloonTipsCheckbox.Checked), settings, nameof(settings.ShowBalloonTips));
            reminderCheckbox.DataBindings.Add(nameof(reminderCheckbox.Checked), settings, nameof(settings.ShowReminder));
            reminderTimeoutTextBox.DataBindings.Add(nameof(reminderTimeoutTextBox.Value), settings, nameof(settings.ReminderTimeoutMinutes));
            reminderTimeoutTextBox.DataBindings.Add(nameof(reminderTimeoutTextBox.Enabled), reminderCheckbox, nameof(reminderCheckbox.Checked));

            defaultSleepModeComboBox.LoadFromEnum<CovfefeSleepMode>();
            defaultSleepModeComboBox.DataBindings.Add(nameof(defaultSleepModeComboBox.SelectedValue), settings, nameof(settings.DefaultSleepMode));

            modeToggleCheckBox.DataBindings.Add(nameof(modeToggleCheckBox.Checked), settings, nameof(settings.DoubleClickToggleEnabled));

            toggleModeComboBox.LoadFromEnum<CovfefeSleepMode>(mode => mode != CovfefeSleepMode.Normal);
            toggleModeComboBox.DataBindings.Add(nameof(toggleModeComboBox.SelectedValue), settings, nameof(settings.DoubleClickSleepMode));
            toggleModeComboBox.DataBindings.Add(nameof(toggleModeComboBox.Enabled), modeToggleCheckBox, nameof(modeToggleCheckBox.Checked));

            reminderTimer.Interval = _settings.ReminderTimeoutMinutes * 60000; // convert minutes to milliseconds
        }

        protected override void SetVisibleCore(bool value)
        {
            // ensure the form does not display on initial start, because we should just show the task icon
            base.SetVisibleCore(!_isInitialLoad && value);
            _isInitialLoad = false;
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void StayAwakeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSleepMode(CovfefeSleepMode.StayAwake);
        }

        private void KeepMonitorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSleepMode(CovfefeSleepMode.MonitorsOn);
        }

        private void NormalOperationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSleepMode(CovfefeSleepMode.Normal);
        }

        private void SetSleepMode(CovfefeSleepMode sleepMode)
        {
            notifyContextMenu.UncheckAllItems();

            // a mode change shows its own balloon, superseding any reminder
            _lastBalloonWasTheReminder = false;

            switch (sleepMode)
            {
                case CovfefeSleepMode.Normal:
                    _sleepManagementService.ClearSleepOverrides();
                    normalOperationToolStripMenuItem.Checked = true;
                    covfefeNotifyIcon.Icon = Resources.coffee_cup_cold;
                    if (_settings.ShowBalloonTips) covfefeNotifyIcon.ShowBalloonTip(10000, BalloonTitle_Normal, BalloonText_Normal, ToolTipIcon.None);
                    break;
                case CovfefeSleepMode.StayAwake:
                    _sleepManagementService.DisableSleep();
                    stayAwakeToolStripMenuItem.Checked = true;
                    covfefeNotifyIcon.Icon = Resources.coffee_cup;
                    if (_settings.ShowBalloonTips) covfefeNotifyIcon.ShowBalloonTip(10000, BalloonTitle_StayAwake, BalloonText_StayAwake, ToolTipIcon.None);
                    break;
                case CovfefeSleepMode.MonitorsOn:
                    _sleepManagementService.KeepMonitorsAwake();
                    keepMonitorsOnToolStripMenuItem.Checked = true;
                    covfefeNotifyIcon.Icon = Resources.coffee_cup;
                    if (_settings.ShowBalloonTips) covfefeNotifyIcon.ShowBalloonTip(10000, BalloonTitle_KeepMonitorsOn, BalloonText_KeepMonitorsOn, ToolTipIcon.None);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sleepMode), sleepMode, null);
            }
            _sleepMode = sleepMode;

            // restart the reminder timer so that the reminder interval starts over
            reminderTimer.Stop();
            if (sleepMode != CovfefeSleepMode.Normal && _settings.ShowReminder) reminderTimer.Start();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
        }

        private void SettingsLabel_MouseDown(object sender, MouseEventArgs e)
        {
            // why can't I just be satisfied with the ugly, default windows border???
            // instead, my settings caption needs to make my settings window draggable
            if (e.Button == MouseButtons.Left)
            {
                WindowsUIApi.ReleaseCapture();
                WindowsUIApi.SendMessage(Handle, WindowsUIApi.WM_NCLBUTTONDOWN, WindowsUIApi.HT_CAPTION, 0);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            _settingsService.SaveSettings(_settings);
            reminderTimer.Interval = _settings.ReminderTimeoutMinutes * 60000; // convert minutes to milliseconds
            reminderTimer.Enabled = _settings.ShowReminder && _sleepMode != CovfefeSleepMode.Normal;
            Hide();
        }

        private void SleepReminderBalloonTip_Click(object s, EventArgs a)
        {
            if (!_lastBalloonWasTheReminder)
                return;

            SetSleepMode(CovfefeSleepMode.Normal);
        }

        private void ReminderTimer_Tick(object sender, EventArgs e)
        {
            _lastBalloonWasTheReminder = true;
            covfefeNotifyIcon.ShowBalloonTip(10000, BalloonTitle_SleepReminder, BalloonText_SleepReminder, ToolTipIcon.None);
        }

        private void CovfefeNotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            if (!_settings.DoubleClickToggleEnabled)
                return;

            if(_sleepMode == CovfefeSleepMode.Normal)
                SetSleepMode(_settings.DoubleClickSleepMode);
            else
                SetSleepMode(CovfefeSleepMode.Normal);
        }

        /// <summary>
        /// Normally, the settings screen will be suppressed when Covfefe launches so that it runs unobtrusively in the system tray.  Call this method prior to launch to request that
        /// the settings be displayed.  Generally, this would be done on the very first run of the app, so that the user is presented with the settings and knows that the app is running.
        /// </summary>
        public void ShowSettingsScreenOnInitialLoad()
        {
            _isInitialLoad = false;
        }
    }
}
