using FinancialAssistant.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.Views
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage(RegisterViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
