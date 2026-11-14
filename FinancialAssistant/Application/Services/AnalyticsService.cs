using System.Globalization;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;

namespace FinancialAssistant.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IFinancialService _financialService;

    public AnalyticsService(IFinancialService financialService)
    {
        _financialService = financialService;
    }

    private static (DateTime Start, DateTime End) GetDateRange(AnalyticsPeriod period)
    {
        var now = DateTime.Now;
        return period switch
        {
            AnalyticsPeriod.Week  => (now.AddDays(-6).Date, now.Date.AddDays(1).AddTicks(-1)),
            AnalyticsPeriod.Month => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1)),
            AnalyticsPeriod.Year  => (new DateTime(now.Year, 1, 1), new DateTime(now.Year + 1, 1, 1).AddTicks(-1)),
            _ => (now.AddDays(-30), now)
        };
    }

    public Task<AnalyticsSummary> GetSummaryAsync(int userId, AnalyticsPeriod period)
    {
        var (start, end) = GetDateRange(period);
        return GetSummaryByRangeAsync(userId, start, end);
    }

    public async Task<AnalyticsSummary> GetSummaryByRangeAsync(int userId, DateTime start, DateTime end)
    {
        var incomes  = await _financialService.GetIncomesAsync(start, end);
        var expenses = await _financialService.GetExpensesAsync(start, end);
        return new AnalyticsSummary
        {
            TotalIncome  = incomes.Sum(i => i.Amount),
            TotalExpense = expenses.Sum(e => e.Amount),
            PeriodStart  = start,
            PeriodEnd    = end
        };
    }

    public Task<List<CategoryBreakdown>> GetExpensesByCategoryAsync(int userId, AnalyticsPeriod period)
    {
        var (start, end) = GetDateRange(period);
        return GetExpensesByCategoryByRangeAsync(userId, start, end);
    }

    public async Task<List<CategoryBreakdown>> GetExpensesByCategoryByRangeAsync(int userId, DateTime start, DateTime end)
    {
        var expenses = await _financialService.GetExpensesAsync(start, end);
        var total = expenses.Sum(e => e.Amount);
        if (total == 0) return new List<CategoryBreakdown>();
        return expenses
            .GroupBy(e => e.CategoryId)
            .Select(g => new CategoryBreakdown
            {
                Category      = g.First().Category ?? new Category { Id = g.Key, Name = "Без категории", Color = "#808080" },
                Amount        = g.Sum(e => e.Amount),
                Percentage    = g.Sum(e => e.Amount) / total * 100,
                ProgressValue = (double)(g.Sum(e => e.Amount) / total)
            })
            .OrderByDescending(c => c.Amount)
            .ToList();
    }

    /// <summary>
    /// Returns category breakdown with trend compared to the previous period of the same length.
    /// </summary>
    public async Task<List<CategoryBreakdown>> GetExpensesByCategoryWithTrendAsync(int userId, DateTime start, DateTime end)
    {
        var span = end - start;
        var prevStart = start - span;
        var prevEnd   = start.AddTicks(-1);

        var currentExpenses  = await _financialService.GetExpensesAsync(start, end);
        var previousExpenses = await _financialService.GetExpensesAsync(prevStart, prevEnd);

        var total = currentExpenses.Sum(e => e.Amount);
        if (total == 0) return new List<CategoryBreakdown>();

        var prevByCategory = previousExpenses
            .GroupBy(e => e.CategoryId)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        return currentExpenses
            .GroupBy(e => e.CategoryId)
            .Select(g =>
            {
                var amount = g.Sum(e => e.Amount);
                prevByCategory.TryGetValue(g.Key, out var prevAmount);

                decimal trendPercent = 0;
                TrendDirection trend = TrendDirection.Neutral;
                if (prevAmount > 0)
                {
                    trendPercent = (amount - prevAmount) / prevAmount * 100;
                    trend = trendPercent > 1m ? TrendDirection.Up
                          : trendPercent < -1m ? TrendDirection.Down
                          : TrendDirection.Neutral;
                }
                else if (amount > 0)
                {
                    trendPercent = 100;
                    trend = TrendDirection.Up;
                }

                return new CategoryBreakdown
                {
                    Category       = g.First().Category ?? new Category { Id = g.Key, Name = "Без категории", Color = "#808080" },
                    Amount         = amount,
                    Percentage     = amount / total * 100,
                    ProgressValue  = (double)(amount / total),
                    PreviousAmount = prevAmount,
                    TrendPercent   = trendPercent,
                    Trend          = trend
                };
            })
            .OrderByDescending(c => c.Amount)
            .ToList();
    }

    public Task<List<ChartDataPoint>> GetTimeSeriesAsync(int userId, AnalyticsPeriod period, TransactionType type)
    {
        var (start, end) = GetDateRange(period);
        return GetTimeSeriesByRangeAsync(userId, start, end, type);
    }

    public async Task<List<ChartDataPoint>> GetTimeSeriesByRangeAsync(int userId, DateTime start, DateTime end, TransactionType type)
    {
        try
        {
            List<(DateTime Date, decimal Amount)> items;
            if (type == TransactionType.Income)
            {
                var incomes = await _financialService.GetIncomesAsync(start, end);
                items = incomes.Select(i => (i.Date, i.Amount)).ToList();
            }
            else
            {
                var expenses = await _financialService.GetExpensesAsync(start, end);
                items = expenses.Select(e => (e.Date, e.Amount)).ToList();
            }

            return GroupBySpan(items, start, end);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnalyticsService] GetTimeSeries error: {ex.Message}");
            return new List<ChartDataPoint>();
        }
    }

    /// <summary>
    /// Returns time-series data filtered by a specific category (expenses only).
    /// </summary>
    public async Task<List<ChartDataPoint>> GetTimeSeriesByCategoryAsync(int userId, DateTime start, DateTime end, int categoryId)
    {
        try
        {
            var expenses = await _financialService.GetExpensesAsync(start, end);
            var items = expenses
                .Where(e => e.CategoryId == categoryId)
                .Select(e => (e.Date, e.Amount))
                .ToList();

            return GroupBySpan(items, start, end);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnalyticsService] GetTimeSeriesByCategory error: {ex.Message}");
            return new List<ChartDataPoint>();
        }
    }

    private static List<ChartDataPoint> GroupBySpan(List<(DateTime Date, decimal Amount)> items, DateTime start, DateTime end)
    {
        if (!items.Any()) return new List<ChartDataPoint>();

        var span = end - start;
        if (span.TotalDays <= 14)
        {
            return items.GroupBy(x => x.Date.Date)
                .Select(g => new ChartDataPoint { Label = g.Key.ToString("dd.MM"), Value = g.Sum(x => x.Amount), Date = g.Key })
                .OrderBy(p => p.Date).ToList();
        }
        else if (span.TotalDays <= 93)
        {
            return items.GroupBy(x => GetSafeIsoWeek(x.Date))
                .Select(g => new ChartDataPoint { Label = "Нед." + g.Key, Value = g.Sum(x => x.Amount), Date = g.Min(x => x.Date) })
                .OrderBy(p => p.Date).ToList();
        }
        else
        {
            return items.GroupBy(x => new { x.Date.Year, x.Date.Month })
                .Select(g => new ChartDataPoint { Label = GetMonthAbbr(g.Key.Month), Value = g.Sum(x => x.Amount), Date = new DateTime(g.Key.Year, g.Key.Month, 1) })
                .OrderBy(p => p.Date).ToList();
        }
    }

    private static int GetSafeIsoWeek(DateTime date)
    {
        try { return ISOWeek.GetWeekOfYear(date); }
        catch { return date.DayOfYear / 7 + 1; }
    }

    public async Task<List<Expense>> GetExpensesByCategoryDetailAsync(int userId, int categoryId, AnalyticsPeriod period)
    {
        var (start, end) = GetDateRange(period);
        return (await _financialService.GetExpensesAsync(start, end))
            .Where(e => e.CategoryId == categoryId)
            .OrderByDescending(e => e.Date)
            .ToList();
    }

    private static string GetMonthAbbr(int month) => month switch
    {
        1 => "Янв", 2 => "Фев", 3 => "Мар", 4 => "Апр",
        5 => "Май", 6 => "Июн", 7 => "Июл", 8 => "Авг",
        9 => "Сен", 10 => "Окт", 11 => "Ноя", 12 => "Дек",
        _ => month.ToString()
    };
}
