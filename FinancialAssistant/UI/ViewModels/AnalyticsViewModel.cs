using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using FinancialAssistant.UI.Views;
using Microcharts;
using SkiaSharp;
using Microsoft.Maui.Controls;
using L = FinancialAssistant.UI.Localization.Localization;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class AnalyticsViewModel : BaseViewModel
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IMLForecastService _mlForecastService;
        private readonly IAuthService _authService;
        private readonly ICurrencyService _currencyService;

        // Period
        [ObservableProperty] public partial AnalyticsPeriod SelectedPeriod { get; set; } = AnalyticsPeriod.Month;
        [ObservableProperty] public partial bool IsPeriodWeek { get; set; }
        [ObservableProperty] public partial bool IsPeriodMonth { get; set; } = true;
        [ObservableProperty] public partial bool IsPeriodYear { get; set; }

        // Custom date range
        [ObservableProperty] public partial DateTime CustomStartDate { get; set; } = DateTime.Now.AddMonths(-1);
        [ObservableProperty] public partial DateTime CustomEndDate { get; set; } = DateTime.Now;
        private bool _useCustomRange = false;

        // Summary
        [ObservableProperty] public partial string TotalIncomeFormatted { get; set; } = "0";
        [ObservableProperty] public partial string TotalExpenseFormatted { get; set; } = "0";
        [ObservableProperty] public partial string BalanceFormatted { get; set; } = "0";
        [ObservableProperty] public partial string CurrencySymbol { get; set; } = "Br";
        [ObservableProperty] public partial Color BalanceColor { get; set; } = Colors.Green;

        // Charts — nulled out before reassignment to force full redraw and prevent overlay
        [ObservableProperty] public partial Chart? TimeSeriesChart { get; set; }
        [ObservableProperty] public partial Chart? CategoryChart { get; set; }
        [ObservableProperty] public partial string ChartTypeLabel { get; set; } = L.T("Analytics_ChartLabelExpenses");

        // Categories
        [ObservableProperty] public partial ObservableCollection<CategoryBreakdown> CategoryBreakdowns { get; set; } = new();

        // Category filter for time-series chart
        [ObservableProperty] public partial ObservableCollection<CategoryFilterItem> CategoryFilters { get; set; } = new();
        [ObservableProperty] public partial CategoryFilterItem? SelectedCategoryFilter { get; set; }
        [ObservableProperty] public partial bool IsCategoryFilterVisible { get; set; }

        // Forecast
        [ObservableProperty] public partial bool IsForecastAvailable { get; set; }
        [ObservableProperty] public partial ObservableCollection<ForecastResult> ForecastResults { get; set; } = new();
        [ObservableProperty] public partial bool IsForecastVisible { get; set; }
        [ObservableProperty] public partial string ForecastHint { get; set; } = string.Empty;

        // State
        [ObservableProperty] public partial bool HasNoData { get; set; }
        [ObservableProperty] public partial bool ShowIncomeChart { get; set; }

        public AnalyticsViewModel(IAnalyticsService analyticsService, IMLForecastService mlForecastService,
            IAuthService authService, ICurrencyService currencyService)
        {
            _analyticsService = analyticsService;
            _mlForecastService = mlForecastService;
            _authService = authService;
            _currencyService = currencyService;
        }

        partial void OnSelectedPeriodChanged(AnalyticsPeriod value)
        {
            _useCustomRange = false;
            IsPeriodWeek  = value == AnalyticsPeriod.Week;
            IsPeriodMonth = value == AnalyticsPeriod.Month;
            IsPeriodYear  = value == AnalyticsPeriod.Year;
            if (!IsBusy) LoadDataCommand.Execute(null);
        }

        partial void OnShowIncomeChartChanged(bool value)
        {
            ChartTypeLabel = value
                ? L.T("Analytics_ChartLabelIncome")
                : L.T("Analytics_ChartLabelExpenses");
            // Hide category filter when showing income (no category breakdown for income)
            if (value) SelectedCategoryFilter = null;
            IsCategoryFilterVisible = !value;
            if (!IsBusy) LoadDataCommand.Execute(null);
        }

        partial void OnSelectedCategoryFilterChanged(CategoryFilterItem? value)
        {
            if (!IsBusy) RefreshTimeSeriesChartCommand.Execute(null);
        }

        [RelayCommand]
        public void ApplyCustomRange()
        {
            _useCustomRange = true;
            IsPeriodWeek = IsPeriodMonth = IsPeriodYear = false;
            LoadDataCommand.Execute(null);
        }

        private (DateTime Start, DateTime End) GetCurrentRange()
        {
            if (_useCustomRange)
                return (CustomStartDate.Date, CustomEndDate.Date.AddDays(1).AddTicks(-1));
            var now = DateTime.Now;
            return SelectedPeriod switch
            {
                AnalyticsPeriod.Week  => (now.AddDays(-6).Date, now.Date.AddDays(1).AddTicks(-1)),
                AnalyticsPeriod.Month => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1)),
                AnalyticsPeriod.Year  => (new DateTime(now.Year, 1, 1), new DateTime(now.Year + 1, 1, 1).AddTicks(-1)),
                _ => (now.AddDays(-30), now)
            };
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            // Null out charts first to prevent overlay on redraw
            TimeSeriesChart = null;
            CategoryChart = null;

            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user is null) { HasNoData = true; return; }

                CurrencySymbol = _currencyService.GetCurrencySymbol(user.Currency ?? "BYN");
                var (start, end) = GetCurrentRange();

                var summary = await _analyticsService.GetSummaryByRangeAsync(user.Id, start, end);

                var income  = await _currencyService.ConvertAsync(summary.TotalIncome,  "BYN", user.Currency ?? "BYN");
                var expense = await _currencyService.ConvertAsync(summary.TotalExpense, "BYN", user.Currency ?? "BYN");
                var balance = income - expense;

                TotalIncomeFormatted  = $"{income:N2} {CurrencySymbol}";
                TotalExpenseFormatted = $"{expense:N2} {CurrencySymbol}";
                BalanceFormatted      = $"{(balance >= 0 ? "+" : "")}{balance:N2} {CurrencySymbol}";
                BalanceColor = balance >= 0 ? Color.FromArgb("#22C55E") : Color.FromArgb("#EF4444");

                // Load breakdowns with trend data
                var breakdowns = await _analyticsService.GetExpensesByCategoryWithTrendAsync(user.Id, start, end);
                CategoryBreakdowns.Clear();
                foreach (var b in breakdowns) CategoryBreakdowns.Add(b);

                // Rebuild category filter list (keep "Все" as first item)
                var previousFilter = SelectedCategoryFilter?.CategoryId;
                CategoryFilters.Clear();
                CategoryFilters.Add(new CategoryFilterItem { CategoryId = null, Name = L.T("Analytics_FilterAll") });
                foreach (var b in breakdowns)
                    CategoryFilters.Add(new CategoryFilterItem { CategoryId = b.Category.Id, Name = b.Category.Name, Color = b.Category.Color });

                // Restore previous selection or default to "Все"
                SelectedCategoryFilter = previousFilter.HasValue
                    ? CategoryFilters.FirstOrDefault(f => f.CategoryId == previousFilter) ?? CategoryFilters[0]
                    : CategoryFilters[0];

                IsCategoryFilterVisible = !ShowIncomeChart && breakdowns.Count > 0;

                // Build time-series chart
                await BuildTimeSeriesChartAsync(user.Id, start, end);

                // Build donut chart
                if (breakdowns.Count > 0)
                {
                    var catEntries = breakdowns.Take(6).Select(b => new ChartEntry((float)b.Amount)
                    {
                        Label = b.Category?.Name ?? "?",
                        ValueLabel = $"{b.Percentage:N0}%",
                        Color = SKColor.Parse(b.Category?.Color ?? "#808080"),
                        TextColor = SKColor.Parse("#94A3B8"),
                        ValueLabelColor = SKColor.Parse(b.Category?.Color ?? "#808080")
                    }).ToList();
                    CategoryChart = new DonutChart
                    {
                        Entries = catEntries,
                        BackgroundColor = SKColors.Transparent,
                        LabelTextSize = 28,
                        HoleRadius = 0.5f
                    };
                }

                IsForecastAvailable = await _mlForecastService.HasSufficientDataAsync(user.Id);
                ForecastHint = IsForecastAvailable
                    ? L.T("Analytics_ForecastHintReady")
                    : L.T("Analytics_ForecastHintInsufficient");

                HasNoData = summary.TotalIncome == 0 && summary.TotalExpense == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Analytics] Error: {ex.Message}");
                HasNoData = true;
            }
            finally { IsBusy = false; }
        }

        /// <summary>Rebuilds only the time-series chart when category filter changes.</summary>
        [RelayCommand]
        public async Task RefreshTimeSeriesChartAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            TimeSeriesChart = null; // clear before redraw
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user is null) return;
                var (start, end) = GetCurrentRange();
                await BuildTimeSeriesChartAsync(user.Id, start, end);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Analytics] RefreshChart error: {ex.Message}");
            }
            finally { IsBusy = false; }
        }

        private async Task BuildTimeSeriesChartAsync(int userId, DateTime start, DateTime end)
        {
            List<ChartDataPoint> points;

            if (!ShowIncomeChart && SelectedCategoryFilter?.CategoryId.HasValue == true)
            {
                // Filtered by specific category
                points = await _analyticsService.GetTimeSeriesByCategoryAsync(userId, start, end, SelectedCategoryFilter.CategoryId!.Value);
            }
            else
            {
                var txType = ShowIncomeChart ? TransactionType.Income : TransactionType.Expense;
                points = await _analyticsService.GetTimeSeriesByRangeAsync(userId, start, end, txType);
            }

            if (points.Count > 0)
            {
                SKColor color;
                if (ShowIncomeChart)
                    color = SKColor.Parse("#22C55E");
                else if (SelectedCategoryFilter?.CategoryId.HasValue == true)
                    color = SKColor.Parse(SelectedCategoryFilter.Color ?? "#EF4444");
                else
                    color = SKColor.Parse("#EF4444");

                // Adaptive sizing: fewer points → larger labels/dots; many points → smaller to avoid overlap
                int count = points.Count;
                float labelSize      = count <= 5  ? 32f
                                     : count <= 10 ? 26f
                                     : count <= 20 ? 20f
                                     : 14f;
                float valueLabelSize = count <= 5  ? 28f
                                     : count <= 10 ? 22f
                                     : count <= 20 ? 16f
                                     : 0f;   // hide value labels when too many points
                float pointSize      = count <= 5  ? 10f
                                     : count <= 10 ? 8f
                                     : count <= 20 ? 6f
                                     : 4f;
                float lineSize       = count <= 10 ? 3f : 2f;

                // For very dense data (>20 pts) switch to bar chart for readability
                Chart chart;
                var entries = points.Select(p => new ChartEntry((float)p.Value)
                {
                    Label = p.Label,
                    ValueLabel = valueLabelSize > 0 ? $"{p.Value:N0}" : string.Empty,
                    Color = color,
                    TextColor = SKColor.Parse("#94A3B8"),
                    ValueLabelColor = color
                }).ToList();

                if (count > 20)
                {
                    chart = new BarChart
                    {
                        Entries = entries,
                        BackgroundColor = SKColors.Transparent,
                        LabelTextSize = labelSize,
                        ValueLabelTextSize = 14f,
                        LabelOrientation = Orientation.Horizontal,
                        ValueLabelOrientation = Orientation.Horizontal
                    };
                }
                else
                {
                    chart = new LineChart
                    {
                        Entries = entries,
                        LineMode = LineMode.Spline,
                        LineSize = lineSize,
                        PointMode = PointMode.Circle,
                        PointSize = pointSize,
                        BackgroundColor = SKColors.Transparent,
                        LabelTextSize = labelSize,
                        ValueLabelTextSize = valueLabelSize
                    };
                }

                TimeSeriesChart = chart;
            }
        }

        [RelayCommand]
        public async Task CalculateForecastAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user is null) return;
                var results = await _mlForecastService.ForecastAsync(user.Id);
                ForecastResults.Clear();
                foreach (var r in results) ForecastResults.Add(r);
                IsForecastVisible = ForecastResults.Count > 0;
                if (!IsForecastVisible)
                    await Shell.Current.DisplayAlertAsync(
                        L.T("Analytics_ForecastAlertTitle"),
                        L.T("Analytics_ForecastNoData"),
                        "OK");
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[Analytics] Forecast error: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        public async Task NavigateToCategoryDetailAsync(CategoryBreakdown breakdown)
        {
            if (breakdown?.Category == null) return;
            await Shell.Current.GoToAsync($"{nameof(CategoryDetailPage)}?categoryId={breakdown.Category.Id}&period={(int)SelectedPeriod}");
        }

        [RelayCommand] public void SetPeriodWeek()  => SelectedPeriod = AnalyticsPeriod.Week;
        [RelayCommand] public void SetPeriodMonth() => SelectedPeriod = AnalyticsPeriod.Month;
        [RelayCommand] public void SetPeriodYear()  => SelectedPeriod = AnalyticsPeriod.Year;
        [RelayCommand] public void ToggleChartType() => ShowIncomeChart = !ShowIncomeChart;
    }

    /// <summary>Item for the category filter picker.</summary>
    public class CategoryFilterItem
    {
        public int? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
    }
}
