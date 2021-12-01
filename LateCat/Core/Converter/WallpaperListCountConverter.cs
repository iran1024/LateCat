using LateCat.PoseidonEngine.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LateCat.Core
{
    public class WallpaperListCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var items = value as ObservableCollection<IWallpaperMetadata>;

            return items?.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}