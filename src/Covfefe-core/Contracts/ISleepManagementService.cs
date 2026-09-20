namespace Covfefe.Contracts
{
    public interface ISleepManagementService
    {
        void KeepMonitorsAwake();
        void DisableSleep();
        void ClearSleepOverrides();
        void DelaySleepOnBatteryStateChange(TimeSpan delay);
        void DelaySleepOnLidClose(TimeSpan delay);
    }
}
