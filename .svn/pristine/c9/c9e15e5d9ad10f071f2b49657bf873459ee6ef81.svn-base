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
            var metadata = (IWallpaperMetadata)value;

            return !string.IsNullOrEmpty(metadata.FilePath) ? new Uri(metadata.FilePath) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
