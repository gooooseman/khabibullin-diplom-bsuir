using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class AddExpenseViewModel : BaseViewModel
    {
        private readonly IFinancialService _financialService;
        private readonly IRepository<Category> _categoryRepo;

        [ObservableProperty]
        public partial decimal Amount { get; set; }

        [ObservableProperty]
        public partial DateTime Date { get; set; } = DateTime.Today;

        [ObservableProperty]
        public partial Category? SelectedCategory { get; set; }

        [ObservableProperty]
        public partial string Comment { get; set; } = string.Empty;

        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<string> Currencies { get; } = new() { "BYN", "USD", "EUR", "RUB" };

        [ObservableProperty]
        public partial string SelectedCurrency { get; set; } = "BYN";

        private readonly IAuthService _authService;
        private readonly ICurrencyService _currencyService;

        public AddExpenseViewModel(IFinancialService financialService, IRepository<Category> categoryRepo,
            IAuthService authService, ICurrencyService currencyService)
        {
            _financialService = financialService;
            _categoryRepo = categoryRepo;
            _authService = authService;
            _currencyService = currencyService;
            SetTitleKey("AddExpense");
            LoadCategories();
            LoadUserCurrency();
        }

        private async void LoadUserCurrency()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user != null) SelectedCurrency = user.Currency ?? "BYN";
        }

        private async void LoadCategories()
        {
            var categories = await _categoryRepo.GetAllAsync();
            Categories.Clear();
            foreach (var category in categories.OrderBy(c => c.Name))
            {
                if (category.Type == TransactionType.Expense)
                    Categories.Add(category);
            }
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

            if (SelectedCategory == null)
            {
                await Shell.Current.DisplayAlertAsync(FinancialAssistant.UI.Localization.Localization.T("Error"),
                    FinancialAssistant.UI.Localization.Localization.T("CategoryRequired"),
                    FinancialAssistant.UI.Localization.Localization.T("OK"));
                return;
            }

            var expense = new Expense
            {
                // Convert amount to BYN for storage
                Amount = await _currencyService.ConvertAsync(Amount, SelectedCurrency, "BYN"),
                Date = Date,
                CategoryId = SelectedCategory.Id,
                Comment = Comment
            };

            await _financialService.AddExpenseAsync(expense);
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task AddCategoryAsync()
        {
            string result = await Shell.Current.DisplayPromptAsync(
                FinancialAssistant.UI.Localization.Localization.T("NewCategory"),
                FinancialAssistant.UI.Localization.Localization.T("EnterCategoryName"),
                FinancialAssistant.UI.Localization.Localization.T("OK"),
                FinancialAssistant.UI.Localization.Localization.T("Cancel"));
            if (!string.IsNullOrWhiteSpace(result))
            {
                var newCategory = new Category
                {
                    Name = result,
                    Type = TransactionType.Expense,
                    Color = "#FF0000", // Default color
                    IsCustom = true
                };
                await _categoryRepo.SaveAsync(newCategory);
                Categories.Add(newCategory);
                SelectedCategory = newCategory;
            }
        }
    }
}
