using FinancialAssistant.UI.ViewModels;

namespace FinancialAssistant.UI.Views;

public partial class AnalyticsPage : ContentPage
{
    public AnalyticsPage(AnalyticsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AnalyticsViewModel vm)
            vm.LoadDataCommand.Execute(null);
    }
}
