using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        }

        protected override void SetVisibleCore(bool value)
        {
            // ensure the form does not display on initial start
            base.SetVisibleCore(!_isInitialLoad && value);
            _isInitialLoad = false;
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void stayAwakeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCheckedState((ToolStripMenuItem)sender);
            _sleepManagementFacade.DisableSleep();
        }

        private void keepMonitorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCheckedState((ToolStripMenuItem)sender);
            _sleepManagementFacade.KeepMonitorsAwake();
        }

        private void normalOperationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCheckedState((ToolStripMenuItem)sender);
            _sleepManagementFacade.ClearSleepOverrides();
        }

        private void SetCheckedState(ToolStripMenuItem checkedItem)
        {
            foreach (object item in notifyContextMenu.Items)
            {
                if (item is ToolStripMenuItem menuItem) 
                    menuItem.Checked = menuItem == checkedItem;
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

        private void TrayForm_Load(object sender, EventArgs e)
        {
            _settings = _settingsFacade.GetSettings();
            startOnLoginCheckbox.DataBindings.Add("Checked", _settings, "StartAtLogin");
        }
    }
}
