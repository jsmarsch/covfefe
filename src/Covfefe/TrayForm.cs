﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Covfefe.Properties;

namespace Covfefe
{
    public partial class TrayForm : Form
    {
        private readonly ISleepManagementFacade _sleepManagementFacade;
        private bool _isInitialLoad = true;
        private readonly ISettingsFacade _settingsFacade;
        private CovfefeSettings _settings;
        private CovfefeSleepMode _sleepMode;

        public TrayForm(ISleepManagementFacade sleepManagementFacade, ISettingsFacade settingsFacade)
        {
            _sleepManagementFacade = sleepManagementFacade;
            _settingsFacade = settingsFacade;
            InitializeComponent();

            _settings = _settingsFacade.GetSettings();
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

        private void stayAwakeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSleepMode(CovfefeSleepMode.StayAwake);
        }

        private void keepMonitorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSleepMode(CovfefeSleepMode.MonitorsOn);
        }

        private void normalOperationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSleepMode(CovfefeSleepMode.Normal);
        }

        private void SetSleepMode(CovfefeSleepMode sleepMode)
        {
            notifyContextMenu.UncheckAllItems();

            switch (sleepMode)
            {
                case CovfefeSleepMode.Normal:
                    _sleepManagementFacade.ClearSleepOverrides();
                    normalOperationToolStripMenuItem.Checked = true;
                    if(_settings.ShowBalloonTips) covfefeNotifyIcon.ShowBalloonTip(10000, Resources.BalloonTitle_Normal, Resources.BalloonText_Normal, ToolTipIcon.None);
                    break;
                case CovfefeSleepMode.StayAwake:
                    _sleepManagementFacade.DisableSleep();
                    stayAwakeToolStripMenuItem.Checked = true;
                    if (_settings.ShowBalloonTips) covfefeNotifyIcon.ShowBalloonTip(10000, Resources.BalloonTitle_StayAwake, Resources.BalloonText_StayAwake, ToolTipIcon.None);
                    break;
                case CovfefeSleepMode.MonitorsOn:
                    _sleepManagementFacade.KeepMonitorsAwake();
                    keepMonitorsOnToolStripMenuItem.Checked = true;
                    if (_settings.ShowBalloonTips) covfefeNotifyIcon.ShowBalloonTip(10000, Resources.BalloonTitle_KeepMonitorsOn, Resources.BalloonText_KeepMonitorsOn, ToolTipIcon.None);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sleepMode), sleepMode, null);
            }
            _sleepMode = sleepMode;

            // restart the reminder timer so that the reminder interval starts over
            reminderTimer.Stop();
            if (sleepMode != CovfefeSleepMode.Normal && _settings.ShowReminder) reminderTimer.Start();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
        }

        private void settingsLabel_MouseDown(object sender, MouseEventArgs e)
        {
            // why can't I just be satisfied with the ugly, default windows border???
            // instead, my settings caption needs to make my settings window draggable
            if (e.Button == MouseButtons.Left)
            {
                WindowsUIApi.ReleaseCapture();
                WindowsUIApi.SendMessage(Handle, WindowsUIApi.WM_NCLBUTTONDOWN, WindowsUIApi.HT_CAPTION, 0);
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            _settingsFacade.SaveSettings(_settings);
            reminderTimer.Interval = _settings.ReminderTimeoutMinutes * 60000; // convert minutes to milliseconds
            reminderTimer.Enabled = _settings.ShowReminder && _sleepMode != CovfefeSleepMode.Normal;
            Hide();
        }

        private void sleepReminderBalloonTip_Click(object s, EventArgs a) => SetSleepMode(CovfefeSleepMode.Normal); 

        private void sleepReminderBalloon_Close(object s, EventArgs a)
        {
            covfefeNotifyIcon.BalloonTipClicked -= sleepReminderBalloonTip_Click;
            covfefeNotifyIcon.BalloonTipClosed -= sleepReminderBalloon_Close;
        }

        private void reminderTimer_Tick(object sender, EventArgs e)
        {
            covfefeNotifyIcon.BalloonTipClicked += sleepReminderBalloonTip_Click;
            covfefeNotifyIcon.BalloonTipClosed += sleepReminderBalloon_Close;

            covfefeNotifyIcon.ShowBalloonTip(10000, Resources.BalloonTitle_SleepReminder, Resources.BalloonText_SleepReminder, ToolTipIcon.None);
        }

        private void covfefeNotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            if (!_settings.DoubleClickToggleEnabled)
                return;

            if(_sleepMode == CovfefeSleepMode.Normal)
                SetSleepMode(_settings.DoubleClickSleepMode);
            else
                SetSleepMode(CovfefeSleepMode.Normal);
        }
    }
}
