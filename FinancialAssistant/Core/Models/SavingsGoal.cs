using SQLite;
using System;

namespace FinancialAssistant.Core.Models
{
    [Table("SavingsGoals")]
    public class SavingsGoal : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime Deadline { get; set; }
        public string Note { get; set; } = string.Empty;
        public int UserId { get; set; }

        [Ignore]
        public double ProgressPercentage => (double)(TargetAmount > 0 ? Math.Min(1, CurrentAmount / TargetAmount) : 0);
        
        [Ignore]
        public decimal RemainingAmount => Math.Max(0, TargetAmount - CurrentAmount);
    }
}
