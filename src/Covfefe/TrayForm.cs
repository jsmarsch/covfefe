using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            // INotify is not implemented on the settings object, so these bindings are one-way
            startOnLoginCheckbox.DataBindings.Add(nameof(startOnLoginCheckbox.Checked), settings, nameof(settings.StartAtLogin));
            showBalloonTipsCheckbox.DataBindings.Add(nameof(showBalloonTipsCheckbox.Checked), settings, nameof(settings.ShowBalloonTips));
            defaultSleepModeComboBox.LoadFromEnum<CovfefeSleepMode>();
            defaultSleepModeComboBox.DataBindings.Add(nameof(defaultSleepModeComboBox.SelectedValue), settings, nameof(settings.DefaultSleepMode));
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
            foreach (object item in notifyContextMenu.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                    menuItem.Checked = false;
            }

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
            Hide();
        }
    }
}
