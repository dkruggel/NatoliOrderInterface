using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace NatoliOrderInterface
{
    class TextBoxValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value.ToString().Length == 0)
                return new ValidationResult(false, "Must have a value here");
            return new ValidationResult(true, null);
        }
    }
}
