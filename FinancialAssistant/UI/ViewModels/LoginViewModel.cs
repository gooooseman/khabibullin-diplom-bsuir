using Microsoft.Extensions.DependencyInjection;
using MauiApp = Microsoft.Maui.Controls.Application;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        public partial string Email { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Password { get; set; } = string.Empty;

        public LoginViewModel(IAuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
            SetTitleKey("Login");
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                return;

            IsBusy = true;
            try
            {
                var (user, error) = await _authService.LoginByEmailAsync(Email, Password);
                if (user != null)
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
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (MauiApp.Current?.Windows.Count > 0 && MauiApp.Current.Windows[0].Page != null)
                            await MauiApp.Current.Windows[0].Page!.DisplayAlertAsync(
                                FinancialAssistant.UI.Localization.Localization.T("Error"),
                                error ?? FinancialAssistant.UI.Localization.Localization.T("LoginFailed"),
                                "OK");
                    });
                }
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        public void GoToRegister()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (MauiApp.Current?.Windows.Count > 0)
                {
                    var page = _serviceProvider.GetRequiredService<RegisterPage>();
                    MauiApp.Current.Windows[0].Page = page;
                }
            });
        }
    }
}
