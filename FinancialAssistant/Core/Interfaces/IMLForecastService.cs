using FinancialAssistant.Core.Models;

namespace FinancialAssistant.Core.Interfaces;

public interface IMLForecastService
{
    Task<List<ForecastResult>> ForecastAsync(int userId);
    Task<bool> HasSufficientDataAsync(int userId);
}
