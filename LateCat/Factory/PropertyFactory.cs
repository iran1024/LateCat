﻿using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using System.IO;

namespace LateCat.Factory
{
    internal class PropertyFactory : IPropertyFactory
    {
        public string CreatePropertyFolder(IWallpaperMetadata metadata, IMonitor monitor, WallpaperArrangement arrangement)
        {
            string propertyPath = null;
            if (!string.IsNullOrEmpty(metadata.PropertyPath))
            {                
                var dataFolder = Program.WallpaperDataDir;
                try
                {                    
                    var screenNumber = monitor.DeviceNumber;
                    if (screenNumber != null)
                    {                        
                        string wpdataFolder = null;
                        switch (arrangement)
                        {
                            case WallpaperArrangement.Per:
                                wpdataFolder = Path.Combine(dataFolder, new DirectoryInfo(metadata.InfoFolderPath).Name, screenNumber);
                                break;
                            case WallpaperArrangement.Span:
                                wpdataFolder = Path.Combine(dataFolder, new DirectoryInfo(metadata.InfoFolderPath).Name, "span");
                                break;
                            case WallpaperArrangement.Duplicate:
                                wpdataFolder = Path.Combine(dataFolder, new DirectoryInfo(metadata.InfoFolderPath).Name, "duplicate");
                                break;
                        }
                        Directory.CreateDirectory(wpdataFolder);
                        
                        propertyPath = Path.Combine(wpdataFolder, "Properties.json");
                        if (!File.Exists(propertyPath))
                        {
                            File.Copy(metadata.PropertyPath, propertyPath);
                        }
                    }
                    else
                    {
                        //todo: fallback, use the original file (restore feature disabled.)
                    }
                }
                catch
                {
                    //todo: fallback, use the original file (restore feature disabled.)
                }
            }
            return propertyPath;
        }
    }
}
