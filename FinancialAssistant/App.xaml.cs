using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.UI.Views;
using FinancialAssistant.Infrastructure.Data;
using System.Globalization;
using FinancialAssistant.UI.Localization;
using Microsoft.Extensions.DependencyInjection;
using FinancialAssistant.Infrastructure.Logging;

namespace FinancialAssistant
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly GlobalExceptionHandler _exceptionHandler;

        public App(IAuthService authService, IServiceProvider serviceProvider, GlobalExceptionHandler exceptionHandler)
        {
            System.Diagnostics.Debug.WriteLine("App Constructor Started");
            
            _exceptionHandler = exceptionHandler;
            _exceptionHandler.Initialize();

            _authService = authService;
            _serviceProvider = serviceProvider;

            try
            {
                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("App InitializeComponent Success");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"App InitializeComponent Failed: {ex}");
                throw;
            }

            // Localization
            Localization.SetCulture(CultureInfo.CurrentUICulture);
            System.Diagnostics.Debug.WriteLine("App Constructor Finished");
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Use LoadingPage as initial page to safely handle async auth check
            try
            {
                var loadingPage = _serviceProvider.GetRequiredService<LoadingPage>();
                return new Window(loadingPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateWindow Failed: {ex}");
                // Fallback to a simple page if DI fails
                return new Window(new ContentPage { Content = new Label { Text = $"Fatal Error: {ex.Message}" } });
            }
        }
    }
}
