using FinancialAssistant.UI.Localization;
using FinancialAssistant.UI.Views;
using Microsoft.Maui.Controls;

namespace FinancialAssistant
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register navigation routes
            Routing.RegisterRoute(nameof(AddIncomePage),      typeof(AddIncomePage));
            Routing.RegisterRoute(nameof(AddExpensePage),     typeof(AddExpensePage));
            Routing.RegisterRoute(nameof(AddGoalPage),        typeof(AddGoalPage));
            Routing.RegisterRoute(nameof(LoginPage),          typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage),       typeof(RegisterPage));
            Routing.RegisterRoute(nameof(PremiumPage),        typeof(PremiumPage));
            Routing.RegisterRoute(nameof(ScanReceiptPage),    typeof(ScanReceiptPage));
            Routing.RegisterRoute(nameof(CategoryDetailPage), typeof(CategoryDetailPage));
            Routing.RegisterRoute(nameof(ProfilePage),        typeof(ProfilePage));

            // Apply translations now and whenever the language changes
            ApplyTabTitles();
            LocalizationSource.Instance.PropertyChanged += (_, _) => ApplyTabTitles();
        }

        private void ApplyTabTitles()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TabHome.Title      = Localization.T("Dashboard");
                TabIncome.Title    = Localization.T("Income");
                TabExpense.Title   = Localization.T("Expense");
                TabAnalytics.Title = Localization.T("Analytics");
                TabGoals.Title     = Localization.T("Goals");
                TabSettings.Title  = Localization.T("Settings");
            });
        }
    }
}
