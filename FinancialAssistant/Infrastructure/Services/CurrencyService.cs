using System.Collections.Generic;
using System.Threading.Tasks;
using FinancialAssistant.Core.Interfaces;

namespace FinancialAssistant.Infrastructure.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IAuthService _authService;

        // Курсы к BYN (апрель 2026, приблизительные)
        // 1 USD ≈ 3.27 BYN, 1 EUR ≈ 3.71 BYN, 1 RUB ≈ 0.034 BYN
        private readonly Dictionary<string, decimal> _ratesFromByn = new()
        {
            { "BYN", 1.0m },
            { "USD", 0.306m },   // 1 BYN = 0.306 USD
            { "EUR", 0.270m },   // 1 BYN = 0.270 EUR
            { "RUB", 29.4m }     // 1 BYN = 29.4 RUB
        };

        private readonly Dictionary<string, string> _currencySymbols = new()
        {
            { "BYN", "Br" },
            { "USD", "$" },
            { "EUR", "€" },
            { "RUB", "₽" }
        };

        public CurrencyService(IAuthService authService)
        {
            _authService = authService;
        }

        public string GetCurrencySymbol(string currencyCode)
        {
            return _currencySymbols.TryGetValue(currencyCode, out var symbol) ? symbol : currencyCode;
        }

        public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            await Task.Yield();

            if (fromCurrency == toCurrency) return amount;

            if (!_ratesFromByn.TryGetValue(fromCurrency, out var fromRate) ||
                !_ratesFromByn.TryGetValue(toCurrency, out var toRate))
                return amount;

            // Convert: amount in fromCurrency → BYN → toCurrency
            decimal amountInByn = fromCurrency == "BYN" ? amount : amount / fromRate;
            decimal result = toCurrency == "BYN" ? amountInByn : amountInByn * toRate;

            return Math.Round(result, 2);
        }

        public async Task<decimal> ConvertToUserCurrencyAsync(decimal amount, string fromCurrency)
        {
            var user = await _authService.GetCurrentUserAsync();
            string targetCurrency = user?.Currency ?? "BYN";
            return await ConvertAsync(amount, fromCurrency, targetCurrency);
        }
    }
}
