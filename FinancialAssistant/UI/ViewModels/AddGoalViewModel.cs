using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class AddGoalViewModel : BaseViewModel
    {
        private readonly IFinancialService _financialService;

        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial decimal TargetAmount { get; set; }

        [ObservableProperty]
        public partial decimal CurrentAmount { get; set; }

        [ObservableProperty]
        public partial DateTime Deadline { get; set; } = DateTime.Today.AddMonths(1);

        public AddGoalViewModel(IFinancialService financialService)
        {
            _financialService = financialService;
            SetTitleKey("AddGoal");
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlertAsync(FinancialAssistant.UI.Localization.Localization.T("Error"),
                    FinancialAssistant.UI.Localization.Localization.T("GoalNameRequired"),
                    FinancialAssistant.UI.Localization.Localization.T("OK"));
                return;
            }

            if (TargetAmount <= 0)
            {
                await Shell.Current.DisplayAlertAsync(FinancialAssistant.UI.Localization.Localization.T("Error"),
                    FinancialAssistant.UI.Localization.Localization.T("TargetAmountPositive"),
                    FinancialAssistant.UI.Localization.Localization.T("OK"));
                return;
            }

            var goal = new SavingsGoal
            {
                Name = Name,
                TargetAmount = TargetAmount,
                CurrentAmount = CurrentAmount,
                Deadline = Deadline
            };

            await _financialService.AddGoalAsync(goal);
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
