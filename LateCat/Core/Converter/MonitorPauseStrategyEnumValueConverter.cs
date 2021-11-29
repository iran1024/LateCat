using LateCat.PoseidonEngine.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LateCat.Core
{
    internal class MonitorPauseStrategyEnumValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null && value is MonitorPauseStrategy strategy)
            {
                return strategy switch
                {
                    MonitorPauseStrategy.All => Properties.Resources.TextDisplayPauseRuleAllMonitor,
                    MonitorPauseStrategy.PerMonitor => Properties.Resources.TextDisplayPauseRulePerMonitor,
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
