namespace FinancialAssistant.Core.Models;

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime Date { get; set; }
}
