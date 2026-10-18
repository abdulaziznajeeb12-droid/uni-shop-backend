
public class Order
{
    public int Id { get; set; }
    public string OrderType { get; set; } = "Normal";   // Normal ya Uniform
    public string Customer { get; set; } = "";
    public string? Organization { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateOnly? DueDate { get; set; }
    public decimal Advance { get; set; }
    public decimal Total { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "";
    public string? Courier { get; set; }
    public string? Tracking { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<OrderItem> Items { get; set; } = new();
    public List<OrderHistory> History { get; set; } = new();
}