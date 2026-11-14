using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using Microsoft.Maui.Controls;
using System.Linq;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class ExpenseViewModel : BaseViewModel
    {
        private readonly IFinancialService _financialService;
        private readonly ICurrencyService _currencyService;
        private readonly IAuthService _authService;

        public ObservableCollection<Expense> Expenses { get; } = new();

        [ObservableProperty]
        public partial string CurrencySymbol { get; set; } = "Br";

        public ExpenseViewModel(IFinancialService financialService, ICurrencyService currencyService, IAuthService authService)
        {
            _financialService = financialService;
            _currencyService = currencyService;
            _authService = authService;
            SetTitleKey("Expense");
        }

        [RelayCommand]
        public async Task LoadExpensesAsync()
        {
            IsBusy = true;
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                string userCurrency = user?.Currency ?? "BYN";
                CurrencySymbol = _currencyService.GetCurrencySymbol(userCurrency);

                var expenses = await _financialService.GetExpensesAsync(DateTime.MinValue, DateTime.MaxValue);
                Expenses.Clear();
                foreach (var expense in expenses.OrderByDescending(e => e.Date))
                {
                    var converted = await _currencyService.ConvertToUserCurrencyAsync(expense.Amount, "BYN");
                    Expenses.Add(new Expense
                    {
                        Id = expense.Id, Amount = converted, Date = expense.Date,
                        Comment = expense.Comment, CategoryId = expense.CategoryId,
                        Category = expense.Category, UserId = expense.UserId
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ExpenseVM] {ex.Message}");
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        public async Task AddExpenseAsync()
        {
            await Shell.Current.GoToAsync(nameof(FinancialAssistant.UI.Views.AddExpensePage));
        }
    }
}
