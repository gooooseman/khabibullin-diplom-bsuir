using System;
using System.Threading.Tasks;
using FinancialAssistant.Core.Interfaces;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace FinancialAssistant.Infrastructure.Logging
{
    public class GlobalExceptionHandler
    {
        private readonly ILoggerService _logger;

        public GlobalExceptionHandler(ILoggerService logger)
        {
            _logger = logger;
        }

        public void Initialize()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                LogUnhandledException(e.ExceptionObject as Exception, "AppDomain.UnhandledException");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception? ex, string source)
        {
            var message = $"CRITICAL ERROR in {source}: {ex?.Message}";
            _logger.LogCritical(message, ex);
            System.Diagnostics.Debug.WriteLine($"[CRASH REPORT] {message}\nStack Trace: {ex?.StackTrace}");
            
            // Optionally notify user on UI thread
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Безопасно получить текущее приложение и страницу, чтобы избежать разыменования null
                var app = Microsoft.Maui.Controls.Application.Current;
                if (app == null)
                    return;

                var windows = app.Windows;
                if (windows == null || windows.Count == 0)
                    return;

                var window = windows[0];
                if (window == null)
                    return;

                var page = window.Page;
                if (page == null)
                    return;

                await page.DisplayAlertAsync("Critical Error", 
                    $"An unexpected error occurred.\n\nSource: {source}\nError: {ex?.Message}", "OK");
            });
        }
    }
}
