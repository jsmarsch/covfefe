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
            SingleInstanceMutex = new Mutex(false, Assembly.GetEntryAssembly().FullName);
            if (!SingleInstanceMutex.WaitOne(0))
                return;

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                // ok, so you don't really need a form to do this -- you could use an ApplicationContext, but I just didn't feel like hand-coding all of the menu and resource stuff, so I used the form and the visual designers

                // todo:  add a container
                var facade = new SleepManagementFacade(new Windows7PowerManagementEngine());
                Application.Run(new TrayForm(facade, new SettingsFacade()));
            }
            finally
            {
                SingleInstanceMutex.ReleaseMutex();
            }
        }

    }
}
