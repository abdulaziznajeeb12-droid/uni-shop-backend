public class StockIn
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int Qty { get; set; }
    public decimal Cost { get; set; }
    public decimal PaidNow { get; set; }
    public DateTime StockDate { get; set; } = DateTime.Now;
}