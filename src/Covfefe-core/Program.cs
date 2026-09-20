using System.Reflection;

namespace Covfefe
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            // only allow 1 instance of this app to run at a time
            using var singleInstanceMutex = new Mutex(false, Assembly.GetEntryAssembly().FullName);
            if (!singleInstanceMutex.WaitOne(0))
                return;

            try
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();

                var sleepManagementService = new SleepManagementService(new Windows7PowerManagementEngine());
                var settingsService = new SettingsService();
                var mainForm = new TrayForm(sleepManagementService, settingsService);

                if (CommandLine.AsksForSettingsScreen(args))
                    mainForm.ShowSettingsScreenOnInitialLoad();

                Application.Run(mainForm);
            }
            finally
            {
                singleInstanceMutex.ReleaseMutex();
            }

        }
    }
}
