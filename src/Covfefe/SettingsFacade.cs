using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Covfefe.Properties;
using Microsoft.Win32;

namespace Covfefe
{
    public class SettingsFacade : ISettingsFacade
    {
        private const string RunKeyName = @"Software\Microsoft\Windows\CurrentVersion\Run";

        private readonly string _exeName;
        private readonly string _appName;

        public SettingsFacade()
        {
            _exeName = Assembly.GetEntryAssembly().Location;
            _appName = Path.GetFileNameWithoutExtension(_exeName);
        }

        public CovfefeSettings GetSettings()
        {
            try
            {
                if (Settings.Default.UpgradeNeeded)
                    UpgradeSettings();

                var result = new CovfefeSettings();
                if (Settings.Default.DefaultSleepMode != null)
                    result.DefaultSleepMode = (CovfefeSleepMode) Enum.Parse(typeof(CovfefeSleepMode), Settings.Default.DefaultSleepMode);
                result.ShowBalloonTips = Settings.Default.ShowBalloonTips;
                result.ShowReminder = Settings.Default.ShowReminder;
                result.ReminderTimeoutMinutes = Settings.Default.ReminderTimeoutMinutes;
                result.DoubleClickToggleEnabled = Settings.Default.DoubleClickToggleEnabled;
                result.DoubleClickSleepMode = (CovfefeSleepMode) Enum.Parse(typeof(CovfefeSleepMode), Settings.Default.DoubleClickSleepMode);

                using (var runKey = Registry.CurrentUser.OpenSubKey(RunKeyName, false))
                {
                    result.StartAtLogin = runKey.GetValueNames().Contains(_appName);
                }
                return result;
            }
            catch
            {
                // intent: if the settings ever fail to load, just re-initialize them and return a default object, instead of crashing
                Settings.Default.Reset();
                return new CovfefeSettings();
            }
        }

        public void SaveSettings(CovfefeSettings settings)
        {
            Settings.Default.DefaultSleepMode = settings.DefaultSleepMode.ToString();
            Settings.Default.ShowBalloonTips = settings.ShowBalloonTips;
            Settings.Default.ShowReminder = settings.ShowReminder;
            Settings.Default.ReminderTimeoutMinutes = settings.ReminderTimeoutMinutes;
            Settings.Default.DoubleClickToggleEnabled = settings.DoubleClickToggleEnabled;
            Settings.Default.DoubleClickSleepMode = settings.DoubleClickSleepMode.ToString();
            Settings.Default.Save();

            using (var runKey = Registry.CurrentUser.OpenSubKey(RunKeyName, true))
            {
                if (settings.StartAtLogin)
                {
                    runKey.SetValue(_appName, _exeName);
                }
                else
                {
                    runKey.DeleteValue(_appName, false);
                }
            }
        }

        /// <summary>
        /// If necessary, upgrade settings so that we don't lose our settings every time the version of the application changes
        /// </summary>
        private void UpgradeSettings()
        {
            Settings.Default.Upgrade();
            Settings.Default.UpgradeNeeded = false;
            Settings.Default.Save();
            Settings.Default.Reload();
        }
    }
}
