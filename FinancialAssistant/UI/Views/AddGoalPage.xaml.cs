using FinancialAssistant.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.Views
{
    public partial class AddGoalPage : ContentPage
    {
        private readonly AddGoalViewModel _viewModel;

        public AddGoalPage(AddGoalViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }
    }
}
