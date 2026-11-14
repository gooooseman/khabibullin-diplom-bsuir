using FinancialAssistant.UI.ViewModels;

namespace FinancialAssistant.UI.Views;

public partial class CategoryDetailPage : ContentPage
{
    public CategoryDetailPage(CategoryDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
