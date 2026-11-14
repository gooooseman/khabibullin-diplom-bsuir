using FinancialAssistant.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FinancialAssistant.Infrastructure.Services
{
    public class LoggerService<T> : ILoggerService
    {
        private readonly ILogger<T> _logger;
        private readonly string _logFilePath;

        public LoggerService(ILogger<T> logger)
        {
            _logger = logger;
            _logFilePath = Path.Combine(FileSystem.AppDataDirectory, "app_logs.txt");
        }

        public void LogInfo(string message)
        {
            _logger.LogInformation(message);
            WriteToFile("INFO", message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
            WriteToFile("WARNING", message);
        }

        public void LogError(string message, Exception? ex = null)
        {
            _logger.LogError(ex, message);
            WriteToFile("ERROR", $"{message} | {ex}");
        }

        public void LogCritical(string message, Exception? ex = null)
        {
            _logger.LogCritical(ex, message);
            WriteToFile("CRITICAL", $"{message} | {ex}");
        }

        private void WriteToFile(string level, string message)
        {
            try
            {
                var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                // Fallback to debug output if file write fails
                System.Diagnostics.Debug.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

        public async Task<string> GetLogContentAsync()
        {
            if (File.Exists(_logFilePath))
            {
                return await File.ReadAllTextAsync(_logFilePath);
            }
            return "No logs found.";
        }

        public void ClearLogs()
        {
            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }
        }
    }
}
