using LateCat.PoseidonEngine.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LateCat.Core
{
    internal class MediaPlayerTypeEnumValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null && value is MediaPlayerType type)
            {
                return type switch
                {
                    MediaPlayerType.Wmf => "WMF",
                    MediaPlayerType.MPV => "MPV",
                    MediaPlayerType.LibMPVExt => "LibMPV（外部进程）",
                    MediaPlayerType.VLC => "Vlc",
                    MediaPlayerType.LibVLCExt => "LibVlc（外部进程）",
                    MediaPlayerType.LibMPV => "LibMPV",
                    MediaPlayerType.LibVLC => "LibVlc",
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
