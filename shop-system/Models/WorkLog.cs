public class WorkLog
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public DateOnly WorkDate { get; set; }
    public string Item { get; set; } = "";
    public int Qty { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}