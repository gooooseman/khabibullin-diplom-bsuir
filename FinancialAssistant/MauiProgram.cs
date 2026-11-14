using Microsoft.Extensions.Logging;
using FinancialAssistant.Infrastructure.Data;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Infrastructure.Repositories;
using FinancialAssistant.Application.Services;
using FinancialAssistant.Infrastructure.Services;
using FinancialAssistant.Infrastructure.Logging;
using FinancialAssistant.UI.ViewModels;
using FinancialAssistant.UI.Views;
using ZXing.Net.Maui.Controls;
using Microcharts.Maui;

namespace FinancialAssistant
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMicrocharts()
                .UseBarcodeReader()
                .ConfigureFonts(fonts => { });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Infrastructure
            builder.Services.AddSingleton(typeof(ILoggerService), typeof(LoggerService<App>));
            builder.Services.AddSingleton<GlobalExceptionHandler>();
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddTransient(typeof(IRepository<>), typeof(GenericRepository<>));

            // Application services
            builder.Services.AddTransient<IFinancialService, FinancialService>();
            builder.Services.AddTransient<IAdviceService, AdviceService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddTransient<ICurrencyService, CurrencyService>();
            builder.Services.AddTransient<IAnalyticsService, AnalyticsService>();
            builder.Services.AddTransient<IMLForecastService, MLForecastService>();
            builder.Services.AddTransient<IQrScannerService, QrScannerService>();
            builder.Services.AddTransient<IReceiptParser, ReceiptParser>();

            // ViewModels
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<IncomeViewModel>();
            builder.Services.AddTransient<ExpenseViewModel>();
            builder.Services.AddTransient<GoalsViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<AddIncomeViewModel>();
            builder.Services.AddTransient<AddExpenseViewModel>();
            builder.Services.AddTransient<AddGoalViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<PremiumViewModel>();
            builder.Services.AddTransient<ScanReceiptViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<AnalyticsViewModel>();
            builder.Services.AddTransient<CategoryDetailViewModel>();
            builder.Services.AddTransient<DebugViewModel>();

            // Pages
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<IncomePage>();
            builder.Services.AddTransient<ExpensePage>();
            builder.Services.AddTransient<GoalsPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<AddIncomePage>();
            builder.Services.AddTransient<AddExpensePage>();
            builder.Services.AddTransient<AddGoalPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<PremiumPage>();
            builder.Services.AddTransient<ScanReceiptPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<LoadingPage>();
            builder.Services.AddTransient<AnalyticsPage>();
            builder.Services.AddTransient<CategoryDetailPage>();
            builder.Services.AddTransient<AppShell>();

            return builder.Build();
        }
    }
}
