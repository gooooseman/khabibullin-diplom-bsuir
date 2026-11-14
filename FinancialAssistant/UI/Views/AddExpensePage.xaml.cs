using FinancialAssistant.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.Views
{
    public partial class AddExpensePage : ContentPage
    {
        private readonly AddExpenseViewModel _viewModel;

        public AddExpensePage(AddExpenseViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }
    }
}
