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

        public TrayForm(ISleepManagementFacade sleepManagementFacade)
        {
            _sleepManagementFacade = sleepManagementFacade;
            InitializeComponent();
        }

        protected override void SetVisibleCore(bool value)
        {
            // ok, so you don't really need a form to host a tray icon -- you could use an ApplicationContext, but I just didn't feel like hand-coding all of the menu and resource stuff, so I used the form and the visual designers
            // so, ensure the form never displays, and doesn't exhibit the annoying "flash" thing
            base.SetVisibleCore(false);
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

        private void delaySleepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCheckedState((ToolStripMenuItem)sender);
            _sleepManagementFacade.DelaySleepOnBatteryStateChange(TimeSpan.FromMinutes(15));
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
    }
}
