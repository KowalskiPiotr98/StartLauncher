using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace StartLauncher.Utilities.Updater
{
    public class UpdateChecker
    {
        private static readonly string _gitHubReleaseUrl = @"https://api.github.com/repos/KowalskiPiotr98/StartLauncher/releases/latest";
        private static readonly string _gitHubReleaseAssetName = @"StartLauncher-win64-installer.msi";

        private bool? updateAvailable;

        public string UpdateDownloadUrl { get; set; }
        public bool UpdateAvailable => updateAvailable.HasValue && updateAvailable.Value;

        public async Task<bool> IsUpdateAvailableAsync()
        {
            var releaseModel = await GetLatestReleaseAsync().ConfigureAwait(false);
            updateAvailable = null;
            if (releaseModel is null)
            {
                throw new UpdateException("Unable to get release from GitHub");
            }
            var releaseVersion = releaseModel.Name.TrimStart('v');
            var releaseVersionNumbers = releaseVersion.Split('.');
            if (releaseVersionNumbers.Length != 3)
            {
                throw new UpdateException("Invalid release format");
            }
            if (!int.TryParse(releaseVersionNumbers[0], out int major) || !int.TryParse(releaseVersionNumbers[1], out int minor) || !int.TryParse(releaseVersionNumbers[2], out int patch))
            {
                throw new UpdateException("Invalid release format");
            }
            if (major > App.Major || minor > App.Minor || patch > App.Patch)
            {
                UpdateDownloadUrl = releaseModel.Assets.FirstOrDefault(u => u.Name == _gitHubReleaseAssetName)?.DownloadUrl;
                if (UpdateDownloadUrl is null)
                {
                    throw new UpdateException("Unable to find installer");
                }
                updateAvailable = true;
            }
            else
            {
                updateAvailable = false;
            }
            return updateAvailable.Value;
        }

        private async Task<GitHubReleaseModel> GetLatestReleaseAsync()
        {
            using var client = new GitHubClient();
            string response;
            try
            {
                var responseMessage = await client.GetAsync(_gitHubReleaseUrl).ConfigureAwait(false);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return null;
                }
                response = await responseMessage.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                return null;
            }
            if (string.IsNullOrEmpty(response))
            {
                return null;
            }
            try
            {
                return JsonSerializer.Deserialize<GitHubReleaseModel>(response);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private class GitHubReleaseModel
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("assets")]
            public GitHubAssetModel[] Assets { get; set; }

        }
        
        private class GitHubAssetModel
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("browser_download_url")]
            public string DownloadUrl { get; set; }
        }

        public static async Task<bool> CheckAndInstallUpdatesAsync()
        {
            var checker = new UpdateChecker();
            if (await checker.IsUpdateAvailableAsync().ConfigureAwait(false))
            {
                var installNowConfirm = MessageBox.Show("New Start Launcher update is available. Download and install now?", "Update available", MessageBoxButton.YesNo);
                if (installNowConfirm == MessageBoxResult.Yes)
                {
                    if (!await UpdateInstaller.TryStartInstallerAsync(checker).ConfigureAwait(false))
                    {
                        MessageBox.Show("Unable to install updates", "Error", MessageBoxButton.OK);
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
