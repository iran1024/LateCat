using LateCat.PoseidonEngine.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LateCat.Core
{
    internal class ProcessMonitorAlgorithmEnumValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null && value is ProcessMonitorAlgorithm algo)
            {
                return algo switch
                {
                    ProcessMonitorAlgorithm.Foreground => Properties.Resources.TextPauseAlgoForegroundProcess,
                    ProcessMonitorAlgorithm.All => Properties.Resources.TextPauseAlgoAllProcess,
                    ProcessMonitorAlgorithm.GameMode => Properties.Resources.TextPauseAlgoExclusiveMode,
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
