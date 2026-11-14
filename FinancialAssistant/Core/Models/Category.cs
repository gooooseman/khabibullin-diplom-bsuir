using SQLite;

namespace FinancialAssistant.Core.Models
{
    public enum TransactionType
    {
        Income,
        Expense
    }

    [Table("Categories")]
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public string Color { get; set; } = "#808080"; // Hex color
        public bool IsCustom { get; set; } = false;
    }
}
