using FinancialAssistant.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.Views
{
    public partial class PremiumPage : ContentPage
    {
        public PremiumPage(PremiumViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
