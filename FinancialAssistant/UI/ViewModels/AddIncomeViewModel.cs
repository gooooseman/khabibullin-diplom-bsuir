using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class AddIncomeViewModel : BaseViewModel
    {
        private readonly IFinancialService _financialService;
        private readonly IAuthService _authService;
        private readonly ICurrencyService _currencyService;

        [ObservableProperty]
        public partial decimal Amount { get; set; }

        [ObservableProperty]
        public partial string Source { get; set; } = string.Empty;

        [ObservableProperty]
        public partial DateTime Date { get; set; } = DateTime.Today;

        [ObservableProperty]
        public partial string Comment { get; set; } = string.Empty;

        public ObservableCollection<string> Currencies { get; } = new() { "BYN", "USD", "EUR", "RUB" };

        [ObservableProperty]
        public partial string SelectedCurrency { get; set; } = "BYN";

        public AddIncomeViewModel(IFinancialService financialService, IAuthService authService, ICurrencyService currencyService)
        {
            _financialService = financialService;
            _authService = authService;
            _currencyService = currencyService;
            SetTitleKey("AddIncome");
            LoadUserCurrency();
        }

        private async void LoadUserCurrency()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user != null) SelectedCurrency = user.Currency ?? "BYN";
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            if (Amount <= 0)
            {
                await Shell.Current.DisplayAlertAsync(FinancialAssistant.UI.Localization.Localization.T("Error"),
                    FinancialAssistant.UI.Localization.Localization.T("AmountMustBePositive"),
                    FinancialAssistant.UI.Localization.Localization.T("OK"));
                return;
            }

            if (string.IsNullOrWhiteSpace(Source))
            {
                await Shell.Current.DisplayAlertAsync(FinancialAssistant.UI.Localization.Localization.T("Error"),
                    FinancialAssistant.UI.Localization.Localization.T("SourceRequired"),
                    FinancialAssistant.UI.Localization.Localization.T("OK"));
                return;
            }

            var income = new Income
            {
                Amount = await _currencyService.ConvertAsync(Amount, SelectedCurrency, "BYN"),
                Source = Source,
                Date = Date,
                Comment = Comment
            };

            await _financialService.AddIncomeAsync(income);
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
