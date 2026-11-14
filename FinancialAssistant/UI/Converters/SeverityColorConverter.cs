using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.Converters
{
    public class SeverityColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string severity)
            {
                return severity switch
                {
                    "Critical" => Colors.Red,
                    "Warning" => Colors.Orange,
                    "Info" => Colors.Blue,
                    _ => Colors.Gray
                };
            }
            return Colors.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
