using LateCat.Helpers;
using LateCat.Models;
using LateCat.PoseidonEngine;
using LateCat.PoseidonEngine.Abstractions;
using System.Collections.Generic;

namespace LateCat.Services
{
    internal class SettingsService : ISettingsService
    {
        private readonly string _settingsPath = Constants.Paths.SettingsPath;
        private readonly string _layoutPath = Constants.Paths.LayoutPath;

        public SettingsService()
        {
            LoadSettings();
            LoadLayouts();
        }

        public ISettings Settings { get; private set; }

        public List<IWallpaperLayout> WallpaperLayouts { get; private set; }

        public void LoadSettings()
        {
            try
            {
                Settings = Json<Settings>.LoadData(_settingsPath);
            }
            catch
            {
                Settings = new Settings();
                SaveSettings();
            }
        }

        public void LoadLayouts()
        {
            try
            {
                WallpaperLayouts = new List<IWallpaperLayout>(Json<List<IWallpaperLayout>>.LoadData(_layoutPath));
            }
            catch
            {
                WallpaperLayouts = new List<IWallpaperLayout>();
                SaveLayouts();
            }
        }

        public void SaveSettings()
        {
            Json<ISettings>.StoreData(_settingsPath, Settings);
        }

        public void SaveLayouts()
        {
            Json<List<IWallpaperLayout>>.StoreData(_layoutPath, WallpaperLayouts);
        }
    }
}
