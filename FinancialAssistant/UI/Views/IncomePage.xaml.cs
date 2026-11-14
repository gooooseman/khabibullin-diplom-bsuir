using FinancialAssistant.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.Views
{
    public partial class IncomePage : ContentPage
    {
        private readonly IncomeViewModel _viewModel;

        public IncomePage(IncomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadIncomesCommand.ExecuteAsync(null);
        }
    }
}
