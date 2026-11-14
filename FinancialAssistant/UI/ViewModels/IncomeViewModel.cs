using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class IncomeViewModel : BaseViewModel
    {
        private readonly IFinancialService _financialService;
        private readonly ICurrencyService _currencyService;
        private readonly IAuthService _authService;

        public ObservableCollection<Income> Incomes { get; } = new();

        [ObservableProperty]
        public partial string CurrencySymbol { get; set; } = "Br";

        public IncomeViewModel(IFinancialService financialService, ICurrencyService currencyService, IAuthService authService)
        {
            _financialService = financialService;
            _currencyService = currencyService;
            _authService = authService;
            SetTitleKey("Income");
        }

        [RelayCommand]
        public async Task LoadIncomesAsync()
        {
            IsBusy = true;
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                string userCurrency = user?.Currency ?? "BYN";
                CurrencySymbol = _currencyService.GetCurrencySymbol(userCurrency);

                var incomes = await _financialService.GetIncomesAsync(DateTime.MinValue, DateTime.MaxValue);
                Incomes.Clear();
                foreach (var income in incomes.OrderByDescending(i => i.Date))
                {
                    var converted = await _currencyService.ConvertToUserCurrencyAsync(income.Amount, "BYN");
                    Incomes.Add(new Income
                    {
                        Id = income.Id, Amount = converted, Source = income.Source,
                        Date = income.Date, Comment = income.Comment, UserId = income.UserId
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[IncomeVM] {ex.Message}");
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        public async Task AddIncomeAsync()
        {
            await Shell.Current.GoToAsync(nameof(FinancialAssistant.UI.Views.AddIncomePage));
        }
    }
}
