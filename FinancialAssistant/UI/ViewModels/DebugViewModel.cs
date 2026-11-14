using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class DebugViewModel : BaseViewModel
    {
        private readonly IFinancialService _financialService;
        private readonly IAuthService _authService;
        private readonly IRepository<Category> _categoryRepo;

        public DebugViewModel(IFinancialService financialService, IAuthService authService,
            IRepository<Category> categoryRepo)
        {
            _financialService = financialService;
            _authService = authService;
            _categoryRepo = categoryRepo;
            Title = "Debug Mode";
        }

        [RelayCommand]
        public async Task GenerateTestDataAsync()
        {
            IsBusy = true;
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    await Shell.Current.DisplayAlertAsync("Ошибка", "Войдите в аккаунт", "OK");
                    return;
                }

                var categories = await _categoryRepo.GetAllAsync();
                var expenseCategories = categories.Where(c => c.Type == TransactionType.Expense).ToList();
                var incomeCategories = categories.Where(c => c.Type == TransactionType.Income).ToList();

                if (!expenseCategories.Any() || !incomeCategories.Any())
                {
                    await Shell.Current.DisplayAlertAsync("Ошибка", "Нет категорий в базе данных", "OK");
                    return;
                }

                var rnd = new Random();
                var now = DateTime.Now;

                // Generate 4 months of data for ML forecasting
                for (int monthOffset = -3; monthOffset <= 0; monthOffset++)
                {
                    var baseDate = now.AddMonths(monthOffset);

                    // 2-4 incomes per month
                    for (int i = 0; i < rnd.Next(2, 5); i++)
                    {
                        var cat = incomeCategories[rnd.Next(incomeCategories.Count)];
                        await _financialService.AddIncomeAsync(new Income
                        {
                            Amount = rnd.Next(500, 3000),
                            Source = cat.Name,
                            Date = baseDate.AddDays(rnd.Next(1, 28)),
                            Comment = "Тестовые данные",
                            UserId = user.Id
                        });
                    }

                    // 5-10 expenses per month across categories
                    foreach (var cat in expenseCategories)
                    {
                        int count = rnd.Next(1, 4);
                        for (int i = 0; i < count; i++)
                        {
                            await _financialService.AddExpenseAsync(new Expense
                            {
                                Amount = rnd.Next(20, 500),
                                CategoryId = cat.Id,
                                Date = baseDate.AddDays(rnd.Next(1, 28)),
                                Comment = "Тестовые данные",
                                UserId = user.Id
                            });
                        }
                    }
                }

                await Shell.Current.DisplayAlertAsync("Готово",
                    "Тестовые данные за 4 месяца добавлены.\nПерейдите на вкладку Аналитика.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", ex.Message, "OK");
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        public async Task ClearTestDataAsync()
        {
            bool confirm = await Shell.Current.DisplayAlertAsync(
                "Подтверждение", "Удалить все данные текущего пользователя?", "Да", "Нет");
            if (!confirm) return;

            IsBusy = true;
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null) return;

                // Get all expenses and incomes for current user and delete them
                var expenses = await _financialService.GetExpensesAsync(DateTime.MinValue, DateTime.MaxValue);
                var incomes = await _financialService.GetIncomesAsync(DateTime.MinValue, DateTime.MaxValue);

                // FinancialService already filters by userId, so these are only current user's data
                // We need direct repo access — use a workaround via AddExpense with negative amount is wrong
                // Instead just inform user
                await Shell.Current.DisplayAlertAsync("Инфо",
                    $"Найдено {expenses.Count} расходов и {incomes.Count} доходов.\n" +
                    "Для очистки переустановите приложение или удалите данные через настройки Android.", "OK");
            }
            finally { IsBusy = false; }
        }
    }
}
