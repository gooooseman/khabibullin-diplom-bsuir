using System.Globalization;
using FinancialAssistant.Core.Models;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.Converters;

/// <summary>Converts TrendDirection to an arrow icon string.</summary>
public class TrendIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is TrendDirection trend ? trend switch
        {
            TrendDirection.Up      => "▲",
            TrendDirection.Down    => "▼",
            TrendDirection.Neutral => "—",
            _ => "—"
        } : "—";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Converts TrendDirection to a color (red for up = more spending, green for down = less spending).</summary>
public class TrendColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is TrendDirection trend ? trend switch
        {
            TrendDirection.Up      => Color.FromArgb("#EF4444"), // red — spending grew
            TrendDirection.Down    => Color.FromArgb("#22C55E"), // green — spending fell
            TrendDirection.Neutral => Color.FromArgb("#94A3B8"), // grey
            _ => Color.FromArgb("#94A3B8")
        } : Color.FromArgb("#94A3B8");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Formats TrendPercent decimal as "+12%" or "-5%" string.</summary>
public class TrendPercentFormatter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal pct)
            return pct >= 0 ? $"+{pct:N0}%" : $"{pct:N0}%";
        return "—";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
