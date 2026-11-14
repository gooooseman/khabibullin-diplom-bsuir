using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using MauiApp = Microsoft.Maui.Controls.Application;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            set
            {
                if (SetProperty(ref _currentUser, value))
                    OnPropertyChanged(nameof(PremiumStatusText));
            }
        }

        public string PremiumStatusText => _currentUser?.IsPremium == true
            ? FinancialAssistant.UI.Localization.Localization.T("Active")
            : FinancialAssistant.UI.Localization.Localization.T("Inactive");

        public ProfileViewModel(IAuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
            SetTitleKey("Profile");
        }

        public async Task LoadAsync()
        {
            var user = await _authService.GetCurrentUserAsync();
            CurrentUser = user;
        }

        [RelayCommand]
        public async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
            var loginPage = _serviceProvider.GetRequiredService<FinancialAssistant.UI.Views.LoginPage>();
            var app = MauiApp.Current;
            if (app?.Windows?.Count > 0)
                app.Windows[0].Page = loginPage;
        }
    }
}
