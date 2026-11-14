namespace FinancialAssistant.Core.Models;

public class ForecastResult
{
    public Category Category { get; set; } = null!;
    public decimal PredictedAmount { get; set; }
    public int MonthsOfData { get; set; }
}
