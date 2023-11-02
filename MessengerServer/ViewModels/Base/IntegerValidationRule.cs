using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MessengerServer.ViewModels
{
    internal class IntegerValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty((string)value))
            {
                return new ValidationResult(false, "Value cannot be empty.");
            }

            if (!int.TryParse((string)value, out int result))
            {
                return new ValidationResult(false, "Value must be an integer.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
