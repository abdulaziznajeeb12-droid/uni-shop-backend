public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public int? ProductId { get; set; }
    public string Name { get; set; } = "";
    public int Qty { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
}