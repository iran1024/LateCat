using System;
using System.IO;

namespace LateCat.PoseidonEngine
{
    internal static class Constants
    {
        internal static class Paths
        {
            public static string AppDataDir { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LateCat");
            public static string WallpaperDir { get; } = Path.Combine(AppDataDir, "wallpapers");
            public static string WallpaperTempDir { get; } = Path.Combine(WallpaperDir, ".late", "temp");
            public static string WallpaperDataDir { get; } = Path.Combine(WallpaperDir, ".late", "data");
            public static string TempDir { get; } = Path.Combine(AppDataDir, "temp");
            public static string TempCefDir { get; } = Path.Combine(AppDataDir, "cef");
            public static string TempWebView2Dir { get; } = Path.Combine(AppDataDir, "webview2");
            public static string LayoutPath { get; } = Path.Combine(AppDataDir, "layout.json");
            public static string SettingsPath { get; } = Path.Combine(AppDataDir, "settings.json");
        }

        internal static class SingleInstance
        {
            public static string UniqueAppName { get; } = "LATECAT:MM-POSEIDON";
            public static string PipeServerName { get; } = UniqueAppName + Environment.UserName;
        }
    }
}
