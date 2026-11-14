using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FinancialAssistant.UI.ViewModels
{
    public class LanguageOption
    {
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>Option for the debug data-period picker.</summary>
    public class DataPeriodOption
    {
        public string DisplayName { get; set; } = string.Empty;
        /// <summary>How many months back to generate data for.</summary>
        public int Months { get; set; }
    }

    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IFinancialService _financialService;
        private readonly IRepository<Category> _categoryRepo;

        // --- Dark Mode ---
        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (SetProperty(ref _isDarkMode, value))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var app = Microsoft.Maui.Controls.Application.Current;
                        if (app != null)
                        {
                            app.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
                            Preferences.Set("AppTheme", value ? "Dark" : "Light");
                        }
                    });
                }
            }
        }

        // --- Currency ---
        public ObservableCollection<string> Currencies { get; } = new() { "BYN", "USD", "EUR", "RUB" };

        private string _selectedCurrency = "BYN";
        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                if (SetProperty(ref _selectedCurrency, value))
                    _ = UpdateCurrencyAsync(value);
            }
        }

        // --- Language ---
        public ObservableCollection<LanguageOption> Languages { get; } = new()
        {
            new LanguageOption { Code = "ru", DisplayName = "Русский" },
            new LanguageOption { Code = "en", DisplayName = "English" }
        };

        private LanguageOption? _selectedLanguage;
        public LanguageOption? SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value) && value != null)
                    ApplyLanguage(value.Code);
            }
        }

        // --- Debug: data period ---
        public ObservableCollection<DataPeriodOption> DataPeriods { get; } = new()
        {
            new DataPeriodOption { DisplayName = "1 неделя",  Months = 0  },   // special: 7 days
            new DataPeriodOption { DisplayName = "1 месяц",   Months = 1  },
            new DataPeriodOption { DisplayName = "3 месяца",  Months = 3  },
            new DataPeriodOption { DisplayName = "6 месяцев", Months = 6  },
            new DataPeriodOption { DisplayName = "1 год",     Months = 12 },
            new DataPeriodOption { DisplayName = "2 года",    Months = 24 },
        };

        private DataPeriodOption? _selectedDataPeriod;
        public DataPeriodOption? SelectedDataPeriod
        {
            get => _selectedDataPeriod;
            set => SetProperty(ref _selectedDataPeriod, value);
        }

        public SettingsViewModel(IAuthService authService, IFinancialService financialService,
            IRepository<Category> categoryRepo)
        {
            _authService = authService;
            _financialService = financialService;
            _categoryRepo = categoryRepo;
            SetTitleKey("Settings");

            // Load theme
            var savedTheme = Preferences.Get("AppTheme", "Light");
            _isDarkMode = savedTheme == "Dark";
            var app = Microsoft.Maui.Controls.Application.Current;
            if (app != null)
                app.UserAppTheme = _isDarkMode ? AppTheme.Dark : AppTheme.Light;

            // Load language
            var savedLang = Preferences.Get("AppLanguage", "ru");
            _selectedLanguage = Languages.FirstOrDefault(l => l.Code == savedLang) ?? Languages[0];
            FinancialAssistant.UI.Localization.Localization.SetCulture(new CultureInfo(savedLang));

            // Default debug period: 3 months
            _selectedDataPeriod = DataPeriods.FirstOrDefault(p => p.Months == 3) ?? DataPeriods[2];

            LoadSettings();
        }

        private async void LoadSettings()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user?.Currency != null)
            {
                _selectedCurrency = user.Currency;
                OnPropertyChanged(nameof(SelectedCurrency));
            }
        }

        private async Task UpdateCurrencyAsync(string currency)
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user != null)
                {
                    user.Currency = currency;
                    await _authService.UpdateUserAsync(user);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Settings] Currency error: {ex.Message}");
            }
        }

        private static void ApplyLanguage(string code)
        {
            try
            {
                Preferences.Set("AppLanguage", code);
                var culture = new CultureInfo(code);
                FinancialAssistant.UI.Localization.Localization.SetCulture(culture);
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Shell.Current != null)
                        await Shell.Current.DisplayAlertAsync(
                            code == "ru" ? "Язык изменён" : "Language changed",
                            code == "ru" ? "Язык изменён. Некоторые строки обновятся сразу, остальные — после перезапуска." 
                                         : "Language changed. Some strings update immediately, others after restart.",
                            "OK");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Settings] Language error: {ex.Message}");
            }
        }

        // ─── Debug Mode ───────────────────────────────────────────────────────────

        [RelayCommand]
        public async Task GoToProfileAsync()
        {
            await Shell.Current.GoToAsync(nameof(FinancialAssistant.UI.Views.ProfilePage));
        }

        [RelayCommand]
        public async Task GenerateTestDataAsync()
        {
            if (IsBusy) return;
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
                var expCats = categories.Where(c => c.Type == TransactionType.Expense).ToList();
                var incCats = categories.Where(c => c.Type == TransactionType.Income).ToList();

                if (!expCats.Any())
                {
                    await Shell.Current.DisplayAlertAsync("Ошибка", "Нет категорий расходов", "OK");
                    return;
                }

                var period = SelectedDataPeriod ?? DataPeriods[2]; // default 3 months
                var rnd = new Random();
                var now = DateTime.Now;

                // Determine date range based on selected period
                DateTime rangeStart;
                int totalMonths;

                if (period.Months == 0)
                {
                    // Special case: 1 week — generate daily data for 7 days
                    rangeStart = now.AddDays(-6).Date;
                    for (int d = 0; d < 7; d++)
                    {
                        var day = rangeStart.AddDays(d);

                        if (incCats.Any() && rnd.Next(0, 3) == 0) // ~33% chance of income per day
                        {
                            await _financialService.AddIncomeAsync(new Income
                            {
                                Amount = rnd.Next(200, 2000),
                                Source = incCats[rnd.Next(incCats.Count)].Name,
                                Date = day,
                                Comment = "Тест",
                                UserId = user.Id
                            });
                        }

                        foreach (var cat in expCats)
                        {
                            if (rnd.Next(0, 2) == 0) // ~50% chance per category per day
                            {
                                await _financialService.AddExpenseAsync(new Expense
                                {
                                    Amount = rnd.Next(10, 200),
                                    CategoryId = cat.Id,
                                    Date = day,
                                    Comment = "Тест",
                                    UserId = user.Id
                                });
                            }
                        }
                    }
                    totalMonths = 0;
                }
                else
                {
                    totalMonths = period.Months;
                    for (int m = -(totalMonths - 1); m <= 0; m++)
                    {
                        var baseDate = now.AddMonths(m);
                        int daysInMonth = DateTime.DaysInMonth(baseDate.Year, baseDate.Month);

                        // Incomes
                        if (incCats.Any())
                        {
                            for (int i = 0; i < rnd.Next(1, 4); i++)
                            {
                                await _financialService.AddIncomeAsync(new Income
                                {
                                    Amount = rnd.Next(800, 3500),
                                    Source = incCats[rnd.Next(incCats.Count)].Name,
                                    Date = new DateTime(baseDate.Year, baseDate.Month, rnd.Next(1, daysInMonth)),
                                    Comment = "Тест",
                                    UserId = user.Id
                                });
                            }
                        }

                        // Expenses per category
                        foreach (var cat in expCats)
                        {
                            int count = rnd.Next(2, 8);
                            for (int i = 0; i < count; i++)
                            {
                                await _financialService.AddExpenseAsync(new Expense
                                {
                                    Amount = rnd.Next(15, 500),
                                    CategoryId = cat.Id,
                                    Date = new DateTime(baseDate.Year, baseDate.Month, rnd.Next(1, daysInMonth)),
                                    Comment = "Тест",
                                    UserId = user.Id
                                });
                            }
                        }
                    }
                }

                string periodLabel = period.DisplayName;
                await Shell.Current.DisplayAlertAsync("✅ Готово",
                    $"Тестовые данные за «{periodLabel}» добавлены.\nПерейдите на вкладку Аналитика.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", ex.Message, "OK");
            }
            finally { IsBusy = false; }
        }
    }
}
