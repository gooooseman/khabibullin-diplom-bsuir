using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using MauiApp = Microsoft.Maui.Controls.Application;

namespace FinancialAssistant.UI.Views
{
    public partial class LoadingPage : ContentPage
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly DatabaseService _databaseService;
        private bool _navigationDone = false;

        public LoadingPage(IAuthService authService, IServiceProvider serviceProvider, DatabaseService databaseService)
        {
            InitializeComponent();
            _authService = authService;
            _serviceProvider = serviceProvider;
            _databaseService = databaseService;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!_navigationDone)
                _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                StatusLabel.Text = "Инициализация базы данных...";
                System.Diagnostics.Debug.WriteLine("[LoadingPage] Init DB start");

                await _databaseService.Init();

                System.Diagnostics.Debug.WriteLine("[LoadingPage] Init DB done");
                StatusLabel.Text = "Проверка сессии...";

                User? user = null;
                try
                {
                    user = await _authService.GetCurrentUserAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoadingPage] GetCurrentUser error: {ex.Message}");
                    user = null;
                }

                System.Diagnostics.Debug.WriteLine($"[LoadingPage] User: {(user != null ? user.Email : "null")}");

                _navigationDone = true;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        if (MauiApp.Current?.Windows.Count > 0)
                        {
                            if (user != null)
                            {
                                var shell = _serviceProvider.GetRequiredService<AppShell>();
                                MauiApp.Current.Windows[0].Page = shell;
                            }
                            else
                            {
                                var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
                                MauiApp.Current.Windows[0].Page = loginPage;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LoadingPage] Navigation error: {ex}");
                        StatusLabel.Text = $"Ошибка навигации: {ex.Message}";
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadingPage] Fatal error: {ex}");
                _navigationDone = true;
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlertAsync("Ошибка запуска", ex.Message, "OK");
                    try
                    {
                        if (MauiApp.Current?.Windows.Count > 0)
                        {
                            var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
                            MauiApp.Current.Windows[0].Page = loginPage;
                        }
                    }
                    catch { }
                });
            }
        }
    }
}
