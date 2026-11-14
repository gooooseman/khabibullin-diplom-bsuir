using FinancialAssistant.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.Views
{
    public partial class ExpensePage : ContentPage
    {
        private readonly ExpenseViewModel _viewModel;

        public ExpensePage(ExpenseViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadExpensesCommand.ExecuteAsync(null);
        }
    }
}
