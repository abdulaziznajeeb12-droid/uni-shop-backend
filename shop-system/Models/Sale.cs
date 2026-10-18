
public class Sale
{
    public int Id { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.Now;
    public string? Customer { get; set; }
    public string Payment { get; set; } = "Cash";
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public decimal Profit { get; set; }
    public bool IsVoid { get; set; }
    public List<SaleItem> Items { get; set; } = new();
}