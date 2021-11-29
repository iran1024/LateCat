using LateCat.PoseidonEngine.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LateCat.Core
{
    internal class GifPlayerEnumValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null && value is GifPlayer player)
            {
                return player switch
                {
                    GifPlayer.MPV => "MPV",
                    GifPlayer.LibMPVExt => "LibMPV（外部进程）",
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
