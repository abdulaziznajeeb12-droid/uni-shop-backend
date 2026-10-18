public class WorkerPayment
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public DateOnly PayDate { get; set; }
    public decimal Amount { get; set; }
    public string Kind { get; set; } = "Salary";   // Salary ya Advance
    public string? Note { get; set; }
    public int? ExpenseId { get; set; }
}