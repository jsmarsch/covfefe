using Covfefe.Contracts;

namespace Covfefe.Tests;

/// <summary>
/// The facade is the app-facing API; the engine behind it holds the P/Invoke calls.
/// These tests pin the mapping from one to the other, which is the whole job of the facade.
/// </summary>
public class SleepManagementFacadeTests
{
    [Test]
    public async Task DisableSleep_asks_the_engine_to_prevent_sleep()
    {
        var engine = new RecordingPowerManagementEngine();
        var facade = new SleepManagementFacade(engine);

        facade.DisableSleep();

        await Assert.That(engine.Calls).IsEquivalentTo(new[] { nameof(IPowerManagementEngine.PreventSleep) });
    }

    [Test]
    public async Task KeepMonitorsAwake_asks_the_engine_to_keep_monitors_on()
    {
        var engine = new RecordingPowerManagementEngine();
        var facade = new SleepManagementFacade(engine);

        facade.KeepMonitorsAwake();

        await Assert.That(engine.Calls).IsEquivalentTo(new[] { nameof(IPowerManagementEngine.KeepMonitorsOn) });
    }

    [Test]
    public async Task ClearSleepOverrides_asks_the_engine_to_clear_its_overrides()
    {
        var engine = new RecordingPowerManagementEngine();
        var facade = new SleepManagementFacade(engine);

        facade.ClearSleepOverrides();

        await Assert.That(engine.Calls)
            .IsEquivalentTo(new[] { nameof(IPowerManagementEngine.ClearPowerManagementOverrides) });
    }

    private sealed class RecordingPowerManagementEngine : IPowerManagementEngine
    {
        public List<string> Calls { get; } = [];

        public void PreventSleep() => Calls.Add(nameof(PreventSleep));

        public void ClearPowerManagementOverrides() => Calls.Add(nameof(ClearPowerManagementOverrides));

        public void KeepMonitorsOn() => Calls.Add(nameof(KeepMonitorsOn));
    }
}
