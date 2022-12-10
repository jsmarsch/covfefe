namespace Covfefe.Contracts
{
    public interface IPowerManagementEngine
    {
        void PreventSleep();
        void ClearPowerManagementOverrides();
        void KeepMonitorsOn();
    }
}