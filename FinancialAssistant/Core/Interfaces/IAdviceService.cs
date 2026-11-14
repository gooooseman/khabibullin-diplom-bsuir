using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinancialAssistant.Core.Interfaces
{
    public class FinancialAdvice
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "Info"; // Info, Warning, Critical
        public string Action { get; set; } = string.Empty;
    }

    public interface IAdviceService
    {
        Task<List<FinancialAdvice>> GenerateAdviceAsync();
    }
}
