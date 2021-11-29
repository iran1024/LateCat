using System;
using System.Globalization;
using System.Windows.Data;

namespace LateCat.Core
{
    public class TaskbarProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var progressValue = 0d;

            if (targetType == typeof(double))
            {
                progressValue = ((double)value) / 100d;
            }

            return progressValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
