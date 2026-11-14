using System.Globalization;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;

namespace FinancialAssistant.Infrastructure.Services;

public class ReceiptParser : IReceiptParser
{
    public (ReceiptData? Data, string? Error) Parse(string qrContent)
    {
        if (string.IsNullOrEmpty(qrContent))
            return (null, "Пустая строка QR-кода");

        var fields = ParseQueryString(qrContent);

        if (!fields.TryGetValue("t", out var t) ||
            !fields.TryGetValue("s", out var s) ||
            !fields.TryGetValue("fn", out var fn) ||
            !fields.TryGetValue("i", out var i) ||
            !fields.TryGetValue("fp", out var fp))
            return (null, "Формат чека не поддерживается");

        DateTime purchaseDate;
        bool parsed = DateTime.TryParseExact(t, "yyyyMMddTHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out purchaseDate)
                   || DateTime.TryParseExact(t, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out purchaseDate);
        if (!parsed)
            return (null, "Некорректная дата в QR-коде");

        if (!decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal totalAmount))
            return (null, "Некорректная сумма в QR-коде");

        return (new ReceiptData
        {
            PurchaseDate = purchaseDate,
            TotalAmount = totalAmount,
            ReceiptId = $"{fn}_{i}_{fp}",
            Items = new List<ReceiptItem>
            {
                new ReceiptItem { Name = "Покупка по чеку", Amount = totalAmount, Quantity = 1 }
            }
        }, null);
    }

    private static Dictionary<string, string> ParseQueryString(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in query.Split('&'))
        {
            var idx = pair.IndexOf('=');
            if (idx > 0)
                result[Uri.UnescapeDataString(pair[..idx])] = Uri.UnescapeDataString(pair[(idx + 1)..]);
        }
        return result;
    }
}
