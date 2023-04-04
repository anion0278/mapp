using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace Shmap.Infrastructure;
public interface IAppCenterIntegration
{
    void TrackException(System.Exception exception);
}

public class AppCenterIntegration : IAppCenterIntegration
{
    public AppCenterIntegration()
    {
        //"9549dd3a-1371-4a23-b973-f5e80154119d"
        //var config = new ConfigurationBuilder().AddUserSecrets<App>().Build();
        //var secretProvider = config.Providers.First();
        //secretProvider.TryGet("appCenterKey", out var appCenterKey); // this requires to have user-secrets defined

        //AppCenter.Start(appCenterKey, typeof(Analytics), typeof(Crashes));
    }

    public void TrackException(System.Exception exception)
    {
        //Crashes.TrackError(exception);
    }
}
