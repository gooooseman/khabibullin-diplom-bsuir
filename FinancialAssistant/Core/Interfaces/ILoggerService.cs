using System;
using System.Threading.Tasks;

namespace FinancialAssistant.Core.Interfaces
{
    public interface ILoggerService
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception? ex = null);
        void LogCritical(string message, Exception? ex = null);
        Task<string> GetLogContentAsync();
        void ClearLogs();
    }
}
