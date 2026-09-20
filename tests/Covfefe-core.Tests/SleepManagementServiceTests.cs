using Covfefe.Contracts;

namespace Covfefe.Tests;

/// <summary>
/// The service is the app-facing API; the engine behind it holds the P/Invoke calls.
/// These tests pin the mapping from one to the other, which is the whole job of the service.
/// </summary>
public class SleepManagementServiceTests
{
    [Test]
    public async Task DisableSleep_asks_the_engine_to_prevent_sleep()
    {
        var engine = new RecordingPowerManagementEngine();
        var service = new SleepManagementService(engine);

        service.DisableSleep();

        await Assert.That(engine.Calls).IsEquivalentTo(new[] { nameof(IPowerManagementEngine.PreventSleep) });
    }

    [Test]
    public async Task KeepMonitorsAwake_asks_the_engine_to_keep_monitors_on()
    {
        var engine = new RecordingPowerManagementEngine();
        var service = new SleepManagementService(engine);

        service.KeepMonitorsAwake();

        await Assert.That(engine.Calls).IsEquivalentTo(new[] { nameof(IPowerManagementEngine.KeepMonitorsOn) });
    }

    [Test]
    public async Task ClearSleepOverrides_asks_the_engine_to_clear_its_overrides()
    {
        var engine = new RecordingPowerManagementEngine();
        var service = new SleepManagementService(engine);

        service.ClearSleepOverrides();

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
