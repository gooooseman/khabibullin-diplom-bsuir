namespace FinancialAssistant.Core.Models;

public class CategoryBreakdown
{
    public Category Category { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public double ProgressValue { get; set; } // 0.0 to 1.0 for ProgressBar

    // Trend compared to previous period
    public decimal PreviousAmount { get; set; }
    public decimal TrendPercent { get; set; }   // positive = growth, negative = decline
    public TrendDirection Trend { get; set; } = TrendDirection.Neutral;
}

public enum TrendDirection { Up, Down, Neutral }
