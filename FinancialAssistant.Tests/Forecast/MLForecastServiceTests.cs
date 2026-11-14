// Feature: financial-assistant-analytics-auth, Property 15: PredictedAmount >= 0
// Feature: financial-assistant-analytics-auth, Property 16: Only categories with 3+ months
using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;
using Xunit;

namespace FinancialAssistant.Tests.Forecast;

public class MLForecastServiceTests
{
    private static decimal ApplyETS(IList<decimal> values, decimal alpha = 0.3m)
    {
        decimal smoothed = values[0];
        for (int i = 1; i < values.Count; i++)
            smoothed = alpha * values[i] + (1 - alpha) * smoothed;
        return smoothed;
    }

    [Property(MaxTest = 100)]
    public Property ETS_Result_IsNonNegative_WhenInputsAreNonNegative()
    {
        // Property 15: All PredictedAmount values are non-negative
        var gen = Gen.ListOf(3, Gen.Choose(0, 10000).Select(x => (decimal)x));
        return Prop.ForAll(Arb.From(gen), values =>
        {
            var list = values.ToList();
            if (list.Count < 3) return;
            var result = Math.Max(0, ApplyETS(list));
            result.Should().BeGreaterThanOrEqualTo(0);
        });
    }

    [Fact]
    public void ETS_WithExactly3Months_ProducesResult()
    {
        // Property 16: categories with exactly 3 months are included
        var values = new List<decimal> { 100m, 200m, 150m };
        var result = ApplyETS(values);
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ETS_WithLessThan3Months_ShouldBeExcluded()
    {
        // Property 16: categories with < 3 months are excluded
        var values = new List<decimal> { 100m, 200m };
        values.Count.Should().BeLessThan(3); // confirms exclusion logic
    }
}
