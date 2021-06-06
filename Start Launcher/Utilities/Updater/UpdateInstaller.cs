using System;
using System.IO;
using System.Threading.Tasks;

namespace StartLauncher.Utilities.Updater
{
    public static class UpdateInstaller
    {
        public static Task<bool> TryStartInstallerAsync(UpdateChecker checker)
        {
            if (!checker.UpdateAvailable)
            {
                throw new ArgumentException("Update is not available", nameof(checker));
            }
            return TryStartInstallerInternalAsync(checker);
        }

        private static async Task<bool> TryStartInstallerInternalAsync(UpdateChecker checker)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            using var client = new GitHubWebClient();
            try
            {
                await client.DownloadFileTaskAsync(new Uri(checker.UpdateDownloadUrl), tempPath);
            }
            catch (UriFormatException)
            {
                File.Delete(tempPath);
                return false;
            }
            try
            {
                File.Move(tempPath, $"{tempPath}.msi");
                tempPath += ".msi";
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = tempPath;
                p.StartInfo.UseShellExecute = true;
                _ = p.Start();
            }
            catch (Exception)
            {
                File.Delete(tempPath);
                return false;
            }
            return true;
        }
    }
}
