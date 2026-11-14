using SQLite;
using System;

namespace FinancialAssistant.Core.Models
{
    [Table("Incomes")]
    public class Income : BaseEntity
    {
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
