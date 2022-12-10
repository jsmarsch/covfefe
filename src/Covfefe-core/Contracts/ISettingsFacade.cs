using Covfefe.Models;

namespace Covfefe.Contracts
{
    public interface ISettingsFacade
    {
        CovfefeSettings GetSettings();
        void SaveSettings(CovfefeSettings settings);
    }
}