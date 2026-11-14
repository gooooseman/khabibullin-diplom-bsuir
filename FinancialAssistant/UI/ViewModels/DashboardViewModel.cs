using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using Microcharts;
using SkiaSharp;
using Microsoft.Maui.Controls;
using L = FinancialAssistant.UI.Localization.Localization;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly IFinancialService _financialService;
        private readonly IAdviceService _adviceService;
        private readonly ICurrencyService _currencyService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        public partial decimal TotalBalance { get; set; }

        [ObservableProperty]
        public partial decimal MonthlyExpense { get; set; }

        [ObservableProperty]
        public partial Chart? ExpenseChart { get; set; }

        [ObservableProperty]
        public partial string CurrencySymbol { get; set; } = "Br";

        /// <summary>Login (Username) of the current user shown in the header.</summary>
        [ObservableProperty]
        public partial string UserLogin { get; set; } = string.Empty;

        /// <summary>Greeting line: "Привет, username" / "Hello, username"</summary>
        [ObservableProperty]
        public partial string UserGreeting { get; set; } = string.Empty;

        public ObservableCollection<FinancialAdvice> AdviceList { get; } = new();

        public DashboardViewModel(IFinancialService financialService, IAdviceService adviceService,
            ICurrencyService currencyService, IAuthService authService)
        {
            _financialService = financialService;
            _adviceService = adviceService;
            _currencyService = currencyService;
            _authService = authService;
            SetTitleKey("Dashboard");
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            IsBusy = true;
            ExpenseChart = null; // clear before redraw to prevent overlay

            try
            {
                var user = await _authService.GetCurrentUserAsync();
                string userCurrency = user?.Currency ?? "BYN";
                CurrencySymbol = _currencyService.GetCurrencySymbol(userCurrency);

                // Show login (Username) in header
                UserLogin = user?.Username ?? string.Empty;
                UserGreeting = $"{L.T("Dashboard_Hello")}, {UserLogin}";

                // Balance
                decimal balanceInBase = await _financialService.GetTotalBalanceAsync();
                TotalBalance = await _currencyService.ConvertToUserCurrencyAsync(balanceInBase, "BYN");

                // Monthly Expense
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                var expenses = await _financialService.GetExpensesAsync(startOfMonth, endOfMonth);
                decimal monthlyExpenseInBase = expenses.Sum(e => e.Amount);
                MonthlyExpense = await _currencyService.ConvertToUserCurrencyAsync(monthlyExpenseInBase, "BYN");

                // Chart
                var expensesByCategory = await _financialService.GetExpensesByCategoryAsync(startOfMonth, endOfMonth);
                var entries = new System.Collections.Generic.List<ChartEntry>();
                foreach (var entry in expensesByCategory)
                {
                    decimal convertedValue = await _currencyService.ConvertToUserCurrencyAsync(entry.Value, "BYN");
                    entries.Add(new ChartEntry((float)convertedValue)
                    {
                        Label = entry.Key,
                        ValueLabel = convertedValue.ToString("N2") + CurrencySymbol,
                        Color = SKColor.Parse("#2196F3")
                    });
                }
                ExpenseChart = new DonutChart { Entries = entries };

                // Advice
                var advice = await _adviceService.GenerateAdviceAsync();
                AdviceList.Clear();
                foreach (var item in advice)
                    AdviceList.Add(item);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to load data: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

