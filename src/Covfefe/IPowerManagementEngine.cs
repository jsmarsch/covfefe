namespace Covfefe
{
    public interface IPowerManagementEngine
    {
        void PreventSleep();
        void ClearPowerManagementOverrides();
        void KeepMonitorsOn();
    }
}