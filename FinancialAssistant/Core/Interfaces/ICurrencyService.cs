using System.Threading.Tasks;

namespace FinancialAssistant.Core.Interfaces
{
    public interface ICurrencyService
    {
        string GetCurrencySymbol(string currencyCode);
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
        Task<decimal> ConvertToUserCurrencyAsync(decimal amount, string fromCurrency);
    }
}
