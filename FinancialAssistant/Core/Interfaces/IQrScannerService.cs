namespace FinancialAssistant.Core.Interfaces;

public interface IQrScannerService
{
    Task<(string? QrContent, string? Error)> ScanAsync();
}
