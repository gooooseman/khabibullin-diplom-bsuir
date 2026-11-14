using SQLite;
using System;

namespace FinancialAssistant.Core.Models
{
    [Table("Expenses")]
    public class Expense : BaseEntity
    {
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int CategoryId { get; set; }
        [Ignore]
        public Category? Category { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string? ReceiptId { get; set; }
    }
}
