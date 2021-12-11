using System.Globalization;
using System.Windows.Controls;

namespace LateCat.Installer.Transitions
{
    class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, "路径是必填的")
                : ValidationResult.ValidResult;
        }
    }
}
