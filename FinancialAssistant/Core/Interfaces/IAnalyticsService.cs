using FinancialAssistant.Core.Models;

namespace FinancialAssistant.Core.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsSummary> GetSummaryAsync(int userId, AnalyticsPeriod period);
    Task<AnalyticsSummary> GetSummaryByRangeAsync(int userId, DateTime start, DateTime end);
    Task<List<CategoryBreakdown>> GetExpensesByCategoryAsync(int userId, AnalyticsPeriod period);
    Task<List<CategoryBreakdown>> GetExpensesByCategoryByRangeAsync(int userId, DateTime start, DateTime end);
    Task<List<CategoryBreakdown>> GetExpensesByCategoryWithTrendAsync(int userId, DateTime start, DateTime end);
    Task<List<ChartDataPoint>> GetTimeSeriesAsync(int userId, AnalyticsPeriod period, TransactionType type);
    Task<List<ChartDataPoint>> GetTimeSeriesByRangeAsync(int userId, DateTime start, DateTime end, TransactionType type);
    Task<List<ChartDataPoint>> GetTimeSeriesByCategoryAsync(int userId, DateTime start, DateTime end, int categoryId);
    Task<List<Expense>> GetExpensesByCategoryDetailAsync(int userId, int categoryId, AnalyticsPeriod period);
}
