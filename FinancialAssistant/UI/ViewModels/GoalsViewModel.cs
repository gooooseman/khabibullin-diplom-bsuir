using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class GoalsViewModel : BaseViewModel
    {
        private readonly IFinancialService _financialService;

        public ObservableCollection<SavingsGoal> Goals { get; } = new();

        public GoalsViewModel(IFinancialService financialService)
        {
            _financialService = financialService;
            SetTitleKey("Goals");
        }

        [RelayCommand]
        public async Task LoadGoalsAsync()
        {
            IsBusy = true;
            try
            {
                var goals = await _financialService.GetGoalsAsync();
                Goals.Clear();
                foreach (var goal in goals) Goals.Add(goal);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[GoalsVM] {ex.Message}"); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        public async Task AddGoalAsync()
        {
            await Shell.Current.GoToAsync(nameof(FinancialAssistant.UI.Views.AddGoalPage));
        }

        [RelayCommand]
        public async Task UpdateGoalAsync(SavingsGoal goal)
        {
            string result = await Shell.Current.DisplayPromptAsync(
                FinancialAssistant.UI.Localization.Localization.T("AddFunds"),
                $"Введите сумму для пополнения «{goal.Name}»:",
                FinancialAssistant.UI.Localization.Localization.T("OK"),
                FinancialAssistant.UI.Localization.Localization.T("Cancel"),
                keyboard: Keyboard.Numeric);
            if (decimal.TryParse(result, out decimal amount) && amount > 0)
            {
                goal.CurrentAmount += amount;
                await _financialService.UpdateGoalAsync(goal);
                await LoadGoalsAsync();
            }
        }
    }
}
