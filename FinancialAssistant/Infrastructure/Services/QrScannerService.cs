using FinancialAssistant.Core.Interfaces;
using Microsoft.Maui.ApplicationModel;

namespace FinancialAssistant.Infrastructure.Services;

public class QrScannerService : IQrScannerService
{
    public async Task<(string? QrContent, string? Error)> ScanAsync()
    {
        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
            return (null, "Для сканирования чека необходим доступ к камере");
        return (null, null); // Permission granted, scanning handled by UI
    }
}
