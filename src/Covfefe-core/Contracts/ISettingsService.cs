using Covfefe.Models;

namespace Covfefe.Contracts
{
    public interface ISettingsService
    {
        CovfefeSettings GetSettings();
        void SaveSettings(CovfefeSettings settings);
    }
}