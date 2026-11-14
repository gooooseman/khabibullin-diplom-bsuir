namespace FinancialAssistant.Core.Interfaces;

public interface IGoogleAuthService
{
    Task<(string? Email, string? DisplayName, string? Error)> AuthenticateAsync();
}
