using System.Diagnostics;
using System;


namespace Shmap.UI;

public interface IBrowserService
{
    void OpenBrowserOnUrl(string url);
}

public class BrowserService : IBrowserService
{
    public void OpenBrowserOnUrl(string url)
    {
        Process.Start(new ProcessStartInfo(url.ToString()) { UseShellExecute = true });
    }
}