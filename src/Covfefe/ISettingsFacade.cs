namespace Covfefe
{
    public interface ISettingsFacade
    {
        CovfefeSettings GetSettings();
        void SaveSettings(CovfefeSettings settings);
    }
}