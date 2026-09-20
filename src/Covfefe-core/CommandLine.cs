namespace Covfefe
{
    /// <summary>
    /// Reads the switches Covfefe accepts on its command line.
    /// </summary>
    public static class CommandLine
    {
        private const string ShowSettingsSwitch = "--ShowSettings";

        /// <summary>
        /// True when the caller asked for the settings screen. The switch is recognised in any
        /// casing and at any position, which is how a command-line switch is expected to behave.
        /// </summary>
        public static bool AsksForSettingsScreen(string[]? args) =>
            args?.Any(argument => string.Equals(argument, ShowSettingsSwitch, StringComparison.OrdinalIgnoreCase)) == true;
    }
}
