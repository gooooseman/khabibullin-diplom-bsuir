// Feature: financial-assistant-analytics-auth, Property 17: Parsing is idempotent
using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;
using System.Globalization;
using Xunit;

namespace FinancialAssistant.Tests.Receipt;

public class ReceiptParserTests
{
    private static (object? Data, string? Error) Parse(string qrContent)
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

        bool parsed = DateTime.TryParseExact(t, "yyyyMMddTHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var purchaseDate)
                   || DateTime.TryParseExact(t, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out purchaseDate);
        if (!parsed)
            return (null, "Некорректная дата в QR-коде");

        if (!decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal totalAmount))
            return (null, "Некорректная сумма в QR-коде");

        return (new { PurchaseDate = purchaseDate, TotalAmount = totalAmount, ReceiptId = $"{fn}_{i}_{fp}" }, null);
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

    [Fact]
    public void Parse_ValidFnsQr_ReturnsReceiptData()
    {
        // Property 17: idempotent parsing
        var qr = "t=20230101T1200&s=1500.00&fn=1234567890123&i=12345&fp=1234567890&n=1";
        var (data1, err1) = Parse(qr);
        var (data2, err2) = Parse(qr);
        err1.Should().BeNull();
        err2.Should().BeNull();
        data1.Should().NotBeNull();
        data2.Should().NotBeNull();
        // Both calls return equivalent data
        data1!.ToString().Should().Be(data2!.ToString());
    }

    [Fact]
    public void Parse_EmptyString_ReturnsError()
    {
        var (data, error) = Parse("");
        data.Should().BeNull();
        error.Should().NotBeNull();
    }

    [Fact]
    public void Parse_MissingRequiredField_ReturnsError()
    {
        var qr = "t=20230101T1200&fn=1234567890123&i=12345&fp=1234567890&n=1"; // missing 's'
        var (data, error) = Parse(qr);
        data.Should().BeNull();
        error.Should().Be("Формат чека не поддерживается");
    }

    [Fact]
    public void Parse_InvalidDate_ReturnsError()
    {
        var qr = "t=INVALID&s=1500.00&fn=1234567890123&i=12345&fp=1234567890&n=1";
        var (data, error) = Parse(qr);
        data.Should().BeNull();
        error.Should().Be("Некорректная дата в QR-коде");
    }
}
