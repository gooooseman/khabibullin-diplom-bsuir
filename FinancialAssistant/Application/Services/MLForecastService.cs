using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;

namespace FinancialAssistant.Application.Services;

public class MLForecastService : IMLForecastService
{
    private readonly IFinancialService _financialService;
    private readonly IRepository<Category> _categoryRepo;

    public MLForecastService(IFinancialService financialService, IRepository<Category> categoryRepo)
    {
        _financialService = financialService;
        _categoryRepo = categoryRepo;
    }

    private static decimal ApplyETS(IList<decimal> values, decimal alpha = 0.3m)
    {
        decimal smoothed = values[0];
        for (int i = 1; i < values.Count; i++)
        {
            smoothed = alpha * values[i] + (1 - alpha) * smoothed;
        }
        return smoothed;
    }

    public async Task<bool> HasSufficientDataAsync(int userId)
    {
        var expenses = await _financialService.GetExpensesAsync(DateTime.Now.AddMonths(-12), DateTime.Now);
        return expenses
            .Where(e => e.UserId == userId)
            .GroupBy(e => e.CategoryId)
            .Any(g => g.Select(e => new { e.Date.Year, e.Date.Month }).Distinct().Count() >= 3);
    }

    public async Task<List<ForecastResult>> ForecastAsync(int userId)
    {
        var results = new List<ForecastResult>();
        var expenses = await _financialService.GetExpensesAsync(DateTime.Now.AddMonths(-12), DateTime.Now);

        var byCategory = expenses
            .Where(e => e.UserId == userId)
            .GroupBy(e => e.CategoryId);

        foreach (var categoryGroup in byCategory)
        {
            try
            {
                int categoryId = categoryGroup.Key;

                var monthlyAmounts = categoryGroup
                    .GroupBy(e => new { e.Date.Year, e.Date.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => g.Sum(e => e.Amount))
                    .ToList();

                if (monthlyAmounts.Count < 3)
                    continue;

                decimal predicted = ApplyETS(monthlyAmounts);
                predicted = Math.Max(0, predicted);

                var category = await _categoryRepo.GetByIdAsync(categoryId);
                if (category is null)
                    continue;

                results.Add(new ForecastResult
                {
                    Category = category,
                    PredictedAmount = predicted,
                    MonthsOfData = monthlyAmounts.Count
                });
            }
            catch
            {
                // Skip categories that fail
            }
        }

        return results;
    }
}
