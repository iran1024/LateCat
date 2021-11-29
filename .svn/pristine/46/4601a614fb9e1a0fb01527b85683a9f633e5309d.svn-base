using LateCat.PoseidonEngine.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LateCat.Core
{
    internal class WallpaperArrangementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (WallpaperArrangement)value == (WallpaperArrangement)int.Parse(parameter.ToString()!);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.Equals(true)
                ? int.Parse(parameter.ToString()!)
                : Binding.DoNothing;
        }
    }
}
