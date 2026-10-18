public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Sku { get; set; }
    public string? Category { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public int Qty { get; set; }
    public int MinQty { get; set; } = 5;
    public DateOnly? ExpiryDate { get; set; }
}