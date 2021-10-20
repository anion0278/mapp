using System;
using System.Linq;
using System.Reflection;
using AutoUpdaterDotNET;
using Shmap.CommonServices;
using Shmap.DataAccess;

namespace Shmap.ApplicationUpdater
{
    public interface IApplicationUpdater
    {
        void CheckUpdate();
    }

    public class ApplicationUpdater : IApplicationUpdater
    {
        private readonly IJsonManager _jsonManager;
        private readonly IDialogService _dialogService;

        public ApplicationUpdater(IJsonManager jsonManager, IDialogService dialogService)
        {
            _jsonManager = jsonManager;
            _dialogService = dialogService;
        }

        public void CheckUpdate()
        {
            // TODO remove admin rules request, OR check if they are needed
            //var ac = new FileInfo(@"Shmap.exe").GetAccessControl();
            //AutoUpdater.RunUpdateAsAdmin = false;

            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.UpdateFormSize = new System.Drawing.Size(600, 400);
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdater_ParseUpdateInfoEvent;
            AutoUpdater.Start("https://mappupdatepackagesrouter.azurewebsites.net/api/UpdatePackagesRouter");
        }

        private void AutoUpdater_ParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            var allUpdates = _jsonManager.DeserializeUpdates(args.RemoteData);

            var applicableUpdates = allUpdates.Where(au =>
                new Version(au.CurrentVersion) > Assembly.GetEntryAssembly().GetName().Version).ToList(); // TODO we should check the same as we display in main Window (Title)

            // TODO handle exception in Autoupdater, that is caused by null in args.UpdateInfo

            if (!applicableUpdates.Any())
            {
                return;
            }

            if (applicableUpdates.Count > 1)
            {
                _dialogService.ShowMessage($"Bylo nalezeno {applicableUpdates.Count} kumulativnich aktualizaci, budou nainstalovany postupne.");
            }

            args.UpdateInfo = applicableUpdates.OrderBy(a => new Version(a.CurrentVersion)).First();
        }
    }
}
