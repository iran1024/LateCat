using LateCat.PoseidonEngine.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LateCat.Core
{
    internal class TaskbarThemeEnumValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null && value is TaskbarTheme theme)
            {
                return theme switch
                {
                    TaskbarTheme.None => Properties.Resources.TextOff,
                    TaskbarTheme.Clear => Properties.Resources.TextTaskbarThemeClear,
                    TaskbarTheme.Blur => Properties.Resources.TextTaskbarThemeBlur,
                    TaskbarTheme.Fluent => Properties.Resources.TextTaskbarThemeFluent,
                    TaskbarTheme.Color => Properties.Resources.TextTaskbarThemeColor,
                    TaskbarTheme.Wallpaper => Properties.Resources.TextTaskbarThemeWallpaper,
                    TaskbarTheme.WallpaperFluent => Properties.Resources.TextTaskbarThemeWallpaperFluent,
                    _ => string.Empty,
                };
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
