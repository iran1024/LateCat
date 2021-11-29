using LateCat.PoseidonEngine.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LateCat.Core
{
    internal class StreamQualitySuggestionEnumValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null && value is StreamQualitySuggestion stream)
            {
                return stream switch
                {
                    StreamQualitySuggestion.Medium => "480p",
                    StreamQualitySuggestion.MediumHigh => "720p",
                    StreamQualitySuggestion.High => "1080p",
                    StreamQualitySuggestion.Highest => "1080p+",
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
