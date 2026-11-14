using Microsoft.Extensions.DependencyInjection;
using MauiApp = Microsoft.Maui.Controls.Application;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty] public partial string DisplayName { get; set; } = string.Empty;
        [ObservableProperty] public partial string Email { get; set; } = string.Empty;
        [ObservableProperty] public partial string Password { get; set; } = string.Empty;
        [ObservableProperty] public partial string ConfirmPassword { get; set; } = string.Empty;

        public RegisterViewModel(IAuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
            SetTitleKey("Register");
        }

        [RelayCommand]
        public async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(DisplayName) || string.IsNullOrWhiteSpace(Password))
            {
                await ShowAlert("Ошибка", "Все поля обязательны"); return;
            }
            if (Password != ConfirmPassword)
            {
                await ShowAlert("Ошибка", "Пароли не совпадают"); return;
            }
            if (Password.Length < 8)
            {
                await ShowAlert("Ошибка", "Пароль должен содержать не менее 8 символов"); return;
            }

            IsBusy = true;
            try
            {
                var (success, error) = await _authService.RegisterAsync(Email, Password, DisplayName);
                if (success)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        if (MauiApp.Current?.Windows.Count > 0)
                        {
                            var shell = _serviceProvider.GetRequiredService<AppShell>();
                            MauiApp.Current.Windows[0].Page = shell;
                        }
                    });
                }
                else
                {
                    await ShowAlert("Ошибка", error ?? "Пользователь уже существует");
                }
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        public void GoToLogin()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (MauiApp.Current?.Windows.Count > 0)
                {
                    var page = _serviceProvider.GetRequiredService<LoginPage>();
                    MauiApp.Current.Windows[0].Page = page;
                }
            });
        }

        private static async Task ShowAlert(string title, string msg)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (MauiApp.Current?.Windows.Count > 0 && MauiApp.Current.Windows[0].Page != null)
                    await MauiApp.Current.Windows[0].Page!.DisplayAlertAsync(title, msg, "OK");
            });
        }
    }
}
