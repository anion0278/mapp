using System.Configuration;

namespace Mapp.UI
{
    public sealed partial class AppSettings
    {
        public bool SettingsFileExistsForCurrentAppVersion()
        {
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).HasFile;
        }

        public void UpgradeSettingsIfRequired()
        {
            if (!SettingsFileExistsForCurrentAppVersion())
            {
                Upgrade();
                Save();
            }
        }
    }
}