using LateCat.PoseidonEngine.Abstractions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LateCat.Core
{
    internal class IWallpaperMetadataConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IWallpaperMetadata metadata)
            {
                return !string.IsNullOrEmpty(metadata.FilePath) ? new Uri(metadata.FilePath) : null;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
