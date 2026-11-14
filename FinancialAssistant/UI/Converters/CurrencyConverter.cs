using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                // In a real app, we'd inject IAuthService or use a static setting
                // For now, let's just use a simple approach or fallback to 'C'
                // If we want to use the selected currency symbol:
                // We need access to the User's currency preference.
                // Since converters are instantiated in Resources, DI is hard.
                
                // Let's stick to standard formatting for now or hardcode BYN if that's the main target
                return amount.ToString("C", CultureInfo.CurrentCulture);
            }
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
