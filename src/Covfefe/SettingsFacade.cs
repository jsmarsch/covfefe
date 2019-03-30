using System;
using System.CodeDom.Compiler;
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
                var result = new CovfefeSettings();
                if (Settings.Default.DefaultSleepMode != null)
                    result.DefaultSleepMode =
                        (CovfefeSleepMode) Enum.Parse(typeof(CovfefeSleepMode), Settings.Default.DefaultSleepMode);
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
            Properties.Settings.Default.DefaultSleepMode = settings.DefaultSleepMode.ToString();
            Properties.Settings.Default.Save();

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
    }
}
