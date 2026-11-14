using FinancialAssistant.Core.Models;

namespace FinancialAssistant.Core.Interfaces;

public interface IReceiptParser
{
    (ReceiptData? Data, string? Error) Parse(string qrContent);
}
