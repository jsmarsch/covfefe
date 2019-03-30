using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Covfefe
{
    static class Program
    {
        private static Mutex SingleInstanceMutex;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // only allow 1 instance of this app to run at a time
            // todo:  I should scope this to the login session, so that there can be multiple instances in multi-session
            SingleInstanceMutex = new Mutex(false, Assembly.GetEntryAssembly().FullName);
            if (!SingleInstanceMutex.WaitOne(0))
                return;

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var sleepManagementFacadefacade = new SleepManagementFacade(new Windows7PowerManagementEngine());
                var settingsFacade = new SettingsFacade();
                var mainForm = new TrayForm(sleepManagementFacadefacade, settingsFacade);
                Application.Run(mainForm);
            }
            finally
            {
                SingleInstanceMutex.ReleaseMutex();
            }
        }

    }
}
