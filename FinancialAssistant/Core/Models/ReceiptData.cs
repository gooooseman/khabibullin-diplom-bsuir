namespace FinancialAssistant.Core.Models;

public class ReceiptData
{
    public DateTime PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ReceiptItem> Items { get; set; } = new();
    public string ReceiptId { get; set; } = string.Empty; // fn_i_fp
}

public class ReceiptItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Quantity { get; set; } = 1;
}
