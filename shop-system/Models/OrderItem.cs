public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int? ProductId { get; set; }   // null = uniform / naap ki line
    public string Name { get; set; } = "";
    public int Qty { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public string? SchoolName { get; set; }     // optional
    public string? StudentName { get; set; }    // optional
    public string? Gender { get; set; }         // Boy / Girl
    public string? Measurements { get; set; }   // jaise "Length 24, Chest 30"
}