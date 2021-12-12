using LateCat.Installer.Models;
using System;
using System.IO;

namespace LateCat.Installer
{
    internal static class Constants
    {
        public static ResourceMap[] Resources =
        {
            new ResourceMap()
            {
                Name = "Program",
                Path = "LateCat.Installer.latecat.latecat.zip",
                DestinationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Late Cat")
            },
            new ResourceMap(){
                Name = "Bundle",
                Path = "LateCat.Installer.bundle.bundle.zip",
                DestinationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Late Cat", "wallpapers")
            }
        };

        public static string InstallerTempDir { get; } = Path.Combine(Path.GetTempPath(), "Late Cat Installer");
    }
}