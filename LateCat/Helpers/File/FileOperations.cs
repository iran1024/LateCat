using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LateCat
{
    public static class FileOperations
    {
        public static void OpenFolder(string path)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe"
            };
            if (File.Exists(path))
            {
                startInfo.Arguments = "/select, \"" + path + "\"";
            }
            else if (Directory.Exists(path))
            {
                startInfo.Arguments = "\"" + path + "\"";
            }
            else
            {
                throw new FileNotFoundException();
            }
            Process.Start(startInfo);
        }

        private static async Task LaunchFolder(string path)
        {
            //var packagePath = path;
            //var localFolder = Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path;
            //var packageAppData = Path.Combine(localFolder, "Local", "LateCat Wallpaper");
            //if (path.Length > Program.AppDataDir.Count() + 1)
            //{
            //    var tmp = Path.Combine(packageAppData, path.Remove(0, Program.AppDataDir.Count() + 1));
            //    if (File.Exists(tmp) || Directory.Exists(tmp))
            //    {
            //        packagePath = tmp;
            //    }
            //}

            //var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(packagePath));
            //await Windows.System.Launcher.LaunchFolderAsync(folder);
        }

        public static string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string NextAvailableFilename(string path)
        {
            if (!File.Exists(path))
                return path;

            var numberPattern = " ({0})";

            if (Path.HasExtension(path))
                return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), numberPattern));

            return GetNextFilename(path + numberPattern);
        }

        private static string GetNextFilename(string pattern)
        {
            string tmp = string.Format(pattern, 1);
            if (tmp == pattern)
                throw new ArgumentException("The pattern must include an index place-holder", "pattern");

            if (!File.Exists(tmp))
                return tmp;

            int min = 1, max = 2;

            while (File.Exists(string.Format(pattern, max)))
            {
                min = max;
                max *= 2;
            }

            while (max != min + 1)
            {
                int pivot = (max + min) / 2;
                if (File.Exists(string.Format(pattern, pivot)))
                    min = pivot;
                else
                    max = pivot;
            }

            return string.Format(pattern, max);
        }

        public static string GetChecksumSHA256(string filePath)
        {
            using SHA256 sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public static bool EmptyDirectory(string directory)
        {
            var status = true;

            try
            {
                var di = new DirectoryInfo(directory);

                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo dir in di.EnumerateDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch
            {
                status = false;
            }

            return status;
        }

        public static async Task<bool> DeleteDirectoryAsync(string folderPath, int initialDelay = 1000, int retryDelay = 4000)
        {
            bool status = true;
            if (Directory.Exists(folderPath))
            {
                await Task.Delay(initialDelay);
                try
                {
                    await Task.Run(() => Directory.Delete(folderPath, true));
                }
                catch
                {
                    await Task.Delay(retryDelay);
                    try
                    {
                        await Task.Run(() => Directory.Delete(folderPath, true));
                    }
                    catch
                    {
                        status = false;
                    }
                }
            }
            return status;
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
