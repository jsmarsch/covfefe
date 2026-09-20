namespace Covfefe.Tests;

/// <summary>
/// Covfefe accepts one switch, --ShowSettings. A command-line switch is expected to work
/// whatever its casing and wherever it sits in the argument list, so these tests pin both.
/// </summary>
public class CommandLineTests
{
    [Test]
    public async Task AsksForSettingsScreen_accepts_the_switch_as_written()
    {
        await Assert.That(CommandLine.AsksForSettingsScreen(["--ShowSettings"])).IsTrue();
    }

    [Test]
    [Arguments("--showsettings")]
    [Arguments("--SHOWSETTINGS")]
    [Arguments("--ShowSETTINGS")]
    public async Task AsksForSettingsScreen_ignores_casing(string argument)
    {
        await Assert.That(CommandLine.AsksForSettingsScreen([argument])).IsTrue();
    }

    [Test]
    public async Task AsksForSettingsScreen_finds_the_switch_after_another_argument()
    {
        await Assert.That(CommandLine.AsksForSettingsScreen(["--something-else", "--showsettings"])).IsTrue();
    }

    [Test]
    public async Task AsksForSettingsScreen_is_false_when_the_switch_is_absent()
    {
        await Assert.That(CommandLine.AsksForSettingsScreen(["--something-else"])).IsFalse();
    }

    [Test]
    public async Task AsksForSettingsScreen_is_false_for_no_arguments()
    {
        await Assert.That(CommandLine.AsksForSettingsScreen([])).IsFalse();
    }

    [Test]
    public async Task AsksForSettingsScreen_is_false_for_a_null_argument_list()
    {
        await Assert.That(CommandLine.AsksForSettingsScreen(null)).IsFalse();
    }

    [Test]
    public async Task AsksForSettingsScreen_does_not_match_a_partial_argument()
    {
        await Assert.That(CommandLine.AsksForSettingsScreen(["--ShowSettingsNow"])).IsFalse();
    }
}
