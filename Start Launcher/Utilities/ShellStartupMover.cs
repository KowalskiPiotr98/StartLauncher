using StartLauncher.PersistentSettings.StartObjects;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StartLauncher.Utilities
{
    public class ShellStartupMover
    {
        private readonly StartObjectsManager _startObjectsManager;

        public ShellStartupMover(StartObjectsManager startObjectsManager)
        {
            _startObjectsManager = startObjectsManager;
        }

        public void MoveAllApplicationsToShellStartup()
        {
            var shellStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            foreach (var app in _startObjectsManager.GetAllStartObjects().Where(a => a is StartApplication))
            {
                var windowsSafeName = app.Location.Split('\\').Last().Replace("exe", "lnk", StringComparison.InvariantCultureIgnoreCase);
                var argumentsString = $"$shell = New-Object -ComObject WScript.Shell; $shortcut = $shell.CreateShortcut(\'{shellStartup}\\{windowsSafeName}\'); $shortcut.TargetPath = \'{app.Location}\'; $shortcut.Save();";
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = argumentsString
                };
                Process.Start(psi); //TODO: well, this is so far from ideal I can barely see it, but there's no real C# way to do this at this point
                _startObjectsManager.RemoveStartObject(app.LaunchOrder);
            }
        }

        public void MoveAllApplicationsFromShellStartup()
        {
            var shellStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            foreach (var file in Directory.GetFiles(shellStartup, "*.lnk"))
            {
                var exePath = GetShortcutTarget(file);
                if (string.IsNullOrEmpty(exePath))
                {
                    continue;
                }
                _startObjectsManager.AddStartObject(new StartApplication(exePath, 1));
                File.Delete(file);
            }
        }

        private static string GetShortcutTarget(string file) // lol (as per: https://www.kittell.net/code/windows-shortcut-lnk-details/)
        {
            try
            {
                FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read);
                using (System.IO.BinaryReader fileReader = new BinaryReader(fileStream))
                {
                    fileStream.Seek(0x14, SeekOrigin.Begin);     // Seek to flags
                    uint flags = fileReader.ReadUInt32();        // Read flags
                    if ((flags & 1) == 1)
                    {                      // Bit 1 set means we have to
                                           // skip the shell item ID list
                        fileStream.Seek(0x4c, SeekOrigin.Begin); // Seek to the end of the header
                        uint offset = fileReader.ReadUInt16();   // Read the length of the Shell item ID list
                        fileStream.Seek(offset, SeekOrigin.Current); // Seek past it (to the file locator info)
                    }

                    long fileInfoStartsAt = fileStream.Position; // Store the offset where the file info
                                                                 // structure begins
                    uint totalStructLength = fileReader.ReadUInt32(); // read the length of the whole struct
                    fileStream.Seek(0xc, SeekOrigin.Current); // seek to offset to base pathname
                    uint fileOffset = fileReader.ReadUInt32(); // read offset to base pathname
                                                               // the offset is from the beginning of the file info struct (fileInfoStartsAt)
                    fileStream.Seek((fileInfoStartsAt + fileOffset), SeekOrigin.Begin); // Seek to beginning of
                                                                                        // base pathname (target)
                    long pathLength = (totalStructLength + fileInfoStartsAt) - fileStream.Position - 2; // read
                                                                                                        // the base pathname. I don't need the 2 terminating nulls.
                    char[] linkTarget = fileReader.ReadChars((int)pathLength); // should be unicode safe
                    var link = new string(linkTarget);

                    int begin = link.IndexOf("\0\0");
                    if (begin > -1)
                    {
                        int end = link.IndexOf("\\\\", begin + 2) + 2;
                        end = link.IndexOf('\0', end) + 1;

                        string firstPart = link.Substring(0, begin);
                        string secondPart = link.Substring(end);

                        return firstPart + secondPart;
                    }
                    else
                    {
                        return link;
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
