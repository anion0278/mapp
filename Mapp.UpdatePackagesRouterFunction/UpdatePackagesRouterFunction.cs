using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Octokit;

namespace UpdatePackagesRouterFunction
{
    public enum Mode
    {
        /// <summary>
        /// In this mode, it ignores Remind Later and Skip values set previously and hide both buttons.
        /// </summary>
        [XmlEnum("0")] Normal,

        /// <summary>
        /// In this mode, it won't show close button in addition to Normal mode behaviour.
        /// </summary>
        [XmlEnum("1")] Forced,

        /// <summary>
        /// In this mode, it will start downloading and applying update without showing standard update dialog in addition to Forced mode behaviour.
        /// </summary>
        [XmlEnum("2")] ForcedDownload,
    }

    public class Mandatory
    {
        /// <summary>
        ///     Value of the Mandatory field.
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        ///     If this is set and 'Value' property is set to true then it will trigger the mandatory update only when current installed version is less than value of this property.
        /// </summary>
        public string MinimumVersion { get; set; }

        /// <summary>
        ///     Mode that should be used for this update.
        /// </summary>
        public Mode UpdateMode { get; set; }
    }

    /// <summary>
    ///     Checksum class to fetch the XML values for checksum.
    /// </summary>
    public class CheckSum
    {
        /// <summary>
        ///     Hash of the file.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Hash algorithm that generated the hash.
        /// </summary>
        public string HashingAlgorithm { get; set; }
    }

    public class UpdateInfoEventArgs : EventArgs
    {
        private string _changelogURL;
        private string _downloadURL;

        /// <inheritdoc />
        public UpdateInfoEventArgs() => this.Mandatory = new Mandatory();

        /// <summary>
        ///     If new update is available then returns true otherwise false.
        /// </summary>
        public bool IsUpdateAvailable { get; set; }

        /// <summary>
        ///     If there is an error while checking for update then this property won't be null.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>Download URL of the update file.</summary>
        public string DownloadURL { get; set; }

        /// <summary>
        ///     URL of the webpage specifying changes in the new update.
        /// </summary>
        public string ChangelogURL { get; set; }

        /// <summary>
        ///     Returns newest version of the application available to download.
        /// </summary>
        public string CurrentVersion { get; set; }

        /// <summary>
        ///     Returns version of the application currently installed on the user's PC.
        /// </summary>
        public Version InstalledVersion { get; set; }

        /// <summary>Shows if the update is required or optional.</summary>
        public Mandatory Mandatory { get; set; }

        /// <summary>Command line arguments used by Installer.</summary>
        public string InstallerArgs { get; set; }

        /// <summary>Checksum of the update file.</summary>
        public CheckSum CheckSum { get; set; }
    }

    public static class UpdatePackagesRouterFunction
    {
        [FunctionName("UpdatePackagesRouter")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function is processing a request");

            var client = new GitHubClient(new ProductHeaderValue("AutomatedUpdatePackagesRouter"));
            var releases = await client.Repository.Release.GetAll("anion0278", "mapp");

            var allReleases = releases.Select(GetUpdateInfo).ToArray();

            var jsonToReturn = JsonSerializer.Serialize(allReleases,
                new JsonSerializerOptions { IgnoreNullValues = true, WriteIndented = true });
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        private static UpdateInfoEventArgs GetUpdateInfo(Release release)
        {
            var binariesAsset = release.Assets.Single(); //.Single(a => a.ContentType.Equals("raw"));
            return new UpdateInfoEventArgs()
            {
                DownloadURL = binariesAsset.BrowserDownloadUrl,
                IsUpdateAvailable = true,
                ChangelogURL = release.HtmlUrl,
                CurrentVersion = release.TagName.Trim('v'),
                Mandatory = new Mandatory() { MinimumVersion = "2.0.0.0", UpdateMode = Mode.Normal, Value = true }
            };
        }
    }
}