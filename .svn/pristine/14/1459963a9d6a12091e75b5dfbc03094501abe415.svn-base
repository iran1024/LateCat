using LateCat.PoseidonEngine.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LateCat.Core
{
    internal class WallpaperScalerEnumValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null && value is WallpaperScaler scaler)
            {
                return scaler switch
                {
                    WallpaperScaler.None => Properties.Resources.TextWallpaperFitNone,
                    WallpaperScaler.Fill => Properties.Resources.TextWallpaperFitFill,
                    WallpaperScaler.Uniform => Properties.Resources.TextWallpaperFitUniform,
                    WallpaperScaler.UniformToFill => Properties.Resources.TextWallpaperFitUniformToFill,
                    WallpaperScaler.Auto => Properties.Resources.TextWallpaperFitAuto,
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
