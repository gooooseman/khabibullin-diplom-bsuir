using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;

namespace FinancialAssistant.UI.ViewModels;

[QueryProperty(nameof(CategoryId), "categoryId")]
[QueryProperty(nameof(PeriodValue), "period")]
public partial class CategoryDetailViewModel : BaseViewModel
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    public partial int CategoryId { get; set; }

    [ObservableProperty]
    public partial int PeriodValue { get; set; }

    [ObservableProperty]
    public partial string CategoryName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<Expense> Expenses { get; set; } = new();

    [ObservableProperty]
    public partial decimal TotalAmount { get; set; }

    public CategoryDetailViewModel(IAnalyticsService analyticsService, IAuthService authService)
    {
        _analyticsService = analyticsService;
        _authService = authService;
    }

    partial void OnCategoryIdChanged(int value)
    {
        if (value > 0)
            LoadDataCommand.Execute(null);
    }

    partial void OnPeriodValueChanged(int value)
    {
        if (CategoryId > 0)
            LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user is null) return;

            var period = (AnalyticsPeriod)PeriodValue;
            var expenses = await _analyticsService.GetExpensesByCategoryDetailAsync(user.Id, CategoryId, period);

            Expenses.Clear();
            foreach (var e in expenses)
                Expenses.Add(e);

            TotalAmount = expenses.Sum(e => e.Amount);
            CategoryName = expenses.FirstOrDefault()?.Category?.Name ?? string.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryDetailViewModel] Error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
