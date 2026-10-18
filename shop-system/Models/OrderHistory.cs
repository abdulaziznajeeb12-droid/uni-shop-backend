public class OrderHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Status { get; set; } = "";
    public DateTime ChangedAt { get; set; } = DateTime.Now;
}