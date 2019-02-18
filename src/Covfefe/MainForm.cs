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
    public partial class MainForm : Form
    {
        private WinApi _winApi;
        private bool _active;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _winApi = new WinApi();
            try
            {
                _winApi.PreventSleep();
                _active = true;
            }
            catch (Exception error)
            {
                MessageBox.Show($@"Error: {error}", "Failed to prevent sleep", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!_active) return;

            try
            {
                _winApi.AllowSleep();
            }
            catch (Exception error)
            {
                MessageBox.Show($@"Failed to re-enabled sleep!  This machine might not sleep.  Error: {error}",
                    "ERROR! Sleep might not function",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
