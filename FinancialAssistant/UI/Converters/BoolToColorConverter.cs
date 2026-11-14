using System.Globalization;
using Microsoft.Maui.Controls;
using MauiApp = Microsoft.Maui.Controls.Application;

namespace FinancialAssistant.UI.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return MauiApp.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#A78BFA")
                : Color.FromArgb("#6C63FF");
        return Color.FromArgb("#64748B");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
