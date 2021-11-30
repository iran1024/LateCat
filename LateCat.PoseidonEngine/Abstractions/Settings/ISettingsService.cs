﻿using LateCat.PoseidonEngine.Models;
using System.Collections.Generic;

namespace LateCat.PoseidonEngine.Abstractions
{
    public interface ISettingsService
    {
        ISettings Settings { get; }

        List<IWallpaperLayout> WallpaperLayouts { get; }

        void SaveSettings();

        void SaveLayouts();

        void LoadSettings();

        void LoadLayouts();
    }
}