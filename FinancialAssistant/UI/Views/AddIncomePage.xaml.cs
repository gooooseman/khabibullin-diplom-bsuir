using FinancialAssistant.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.Views
{
    public partial class AddIncomePage : ContentPage
    {
        private readonly AddIncomeViewModel _viewModel;

        public AddIncomePage(AddIncomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }
    }
}
