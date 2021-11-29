using LateCat.PoseidonEngine.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LateCat.Core
{
    internal class InputForwardModeEnumValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null && value is InputForwardMode mode)
            {
                return mode switch
                {
                    InputForwardMode.Off => Properties.Resources.TextOff,
                    InputForwardMode.Mouse => Properties.Resources.TextMouse,
                    InputForwardMode.MouseKeyboard => Properties.Resources.TextKeyboard,
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
