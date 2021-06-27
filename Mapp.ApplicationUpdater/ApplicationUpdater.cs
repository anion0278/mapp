using System;
using System.Linq;
using System.Reflection;
using AutoUpdaterDotNET;
using Shmap.CommonServices;
using Shmap.DataAccess;

namespace Shmap.ApplicationUpdater
{
    public class ApplicationUpdater : IInteractionRequester
    {
        private readonly IJsonManager _jsonManager;
        public EventHandler<string> UserNotification { get; init; }
        public EventHandler<string> UserInteraction { get; init; }


        public ApplicationUpdater(IJsonManager jsonManager)
        {
            _jsonManager = jsonManager;
        }

        public void CheckUpdate()
        {
            // TODO remove admin rules request, OR check if they are needed
            //var ac = new FileInfo(@"Shmap.exe").GetAccessControl();
            //AutoUpdater.RunUpdateAsAdmin = false;

            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.UpdateFormSize = new System.Drawing.Size(600, 400);
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdater_ParseUpdateInfoEvent;
            AutoUpdater.Start("https://raw.githubusercontent.com/anion0278/mapp/master/UpdatesDefinitions.json");
        }


        private void AutoUpdater_ParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            var allUpdates = _jsonManager.DeserializeUpdates(args.RemoteData);

            var applicableUpdates = allUpdates.Where(au =>
                new Version(au.CurrentVersion) > Assembly.GetEntryAssembly().GetName().Version).ToList();

            if (!applicableUpdates.Any())
            {
                return;
            }

            if (applicableUpdates.Count > 1)
            {
                UserNotification.Invoke(this, $"Bylo nalezeno {applicableUpdates.Count} kumulativnich aktualizaci, budou nainstalovany postupne.");
                //MessageBox.Show(
                //    $"Bylo nalezeno {applicableUpdates.Count} kumulativnich aktualizaci, budou nainstalovany postupne.");
            }

            args.UpdateInfo = applicableUpdates.OrderBy(a => new Version(a.CurrentVersion)).First();
        }
    }
}
