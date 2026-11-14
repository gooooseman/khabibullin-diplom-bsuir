using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class PremiumViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private bool isPremium;

        public PremiumViewModel(IAuthService authService)
        {
            _authService = authService;
            SetTitleKey("Premium");
            CheckStatus();
        }

        private async void CheckStatus()
        {
            var user = await _authService.GetCurrentUserAsync();
            IsPremium = user?.IsPremium ?? false;
        }

        [RelayCommand]
        public async Task BuyPremiumAsync()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user != null)
            {
                // Simulate payment
                await Task.Delay(1000); 
                user.IsPremium = true;
                await _authService.UpdateUserAsync(user);
                IsPremium = true;
                await Shell.Current.DisplayAlertAsync("Success", FinancialAssistant.UI.Localization.Localization.T("PremiumActivated"), "OK");
            }
        }
    }
}
