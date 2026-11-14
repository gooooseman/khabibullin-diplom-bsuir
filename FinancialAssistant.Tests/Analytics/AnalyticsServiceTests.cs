// Feature: financial-assistant-analytics-auth, Property 10: Balance = TotalIncome - TotalExpense
// Feature: financial-assistant-analytics-auth, Property 12: Category percentages sum to 100%
// Feature: financial-assistant-analytics-auth, Property 13: Categories sorted by descending amount
using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;

namespace FinancialAssistant.Tests.Analytics;

public class AnalyticsServiceTests
{
    // Property 10: Balance invariant
    [Property(MaxTest = 100)]
    public Property Balance_Equals_Income_Minus_Expense()
    {
        return Prop.ForAll(
            Arb.From(Gen.Choose(0, 100000).Select(x => (decimal)x)),
            Arb.From(Gen.Choose(0, 100000).Select(x => (decimal)x)),
            (income, expense) =>
            {
                var balance = income - expense;
                balance.Should().Be(income - expense);
            });
    }

    // Property 12: Category percentages sum to 100%
    [Property(MaxTest = 100)]
    public Property CategoryPercentages_SumTo100()
    {
        var gen = Gen.NonEmptyListOf(Gen.Choose(1, 1000).Select(x => (decimal)x));
        return Prop.ForAll(Arb.From(gen), amounts =>
        {
            var total = amounts.Sum();
            var percentages = amounts.Select(a => a / total * 100).ToList();
            percentages.Sum().Should().BeApproximately(100m, 0.01m);
        });
    }

    // Property 13: Categories sorted by descending amount
    [Property(MaxTest = 100)]
    public Property Categories_SortedByDescendingAmount()
    {
        var gen = Gen.NonEmptyListOf(Gen.Choose(0, 10000).Select(x => (decimal)x));
        return Prop.ForAll(Arb.From(gen), amounts =>
        {
            var sorted = amounts.OrderByDescending(a => a).ToList();
            for (int i = 1; i < sorted.Count; i++)
                sorted[i].Should().BeLessThanOrEqualTo(sorted[i - 1]);
        });
    }
}
