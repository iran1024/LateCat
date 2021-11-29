using LateCat.PoseidonEngine.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LateCat.Core
{
    internal class PerformanceStrategyEnumValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null && value is PerformanceStrategy strategy)
            {
                return strategy switch
                {
                    PerformanceStrategy.Keep => Properties.Resources.TextPerformanceNone,
                    PerformanceStrategy.Kill => Properties.Resources.TextPerformanceKill,
                    PerformanceStrategy.Pause => Properties.Resources.TextPerformancePause,
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
