using System;
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
            var tempPath = System.IO.Path.GetTempFileName();
            using var client = new GitHubWebClient();
            try
            {
                await client.DownloadFileTaskAsync(new Uri(checker.UpdateDownloadUrl), tempPath);
            }
            catch (UriFormatException)
            {
                System.IO.File.Delete(tempPath);
                return false;
            }
            try
            {
                System.IO.File.Move(tempPath, $"{tempPath}.msi");
                tempPath += ".msi";
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = tempPath;
                p.StartInfo.UseShellExecute = true;
                p.Start();
            }
            catch (Exception)
            {
                System.IO.File.Delete(tempPath);
                return false;
            }
            return true;
        }
    }
}
