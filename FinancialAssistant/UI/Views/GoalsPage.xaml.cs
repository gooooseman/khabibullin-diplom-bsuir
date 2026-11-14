using FinancialAssistant.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.Views
{
    public partial class GoalsPage : ContentPage
    {
        private readonly GoalsViewModel _viewModel;

        public GoalsPage(GoalsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadGoalsAsync();
        }
    }
}
