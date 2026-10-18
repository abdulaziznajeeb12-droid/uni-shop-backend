public class Worker
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string PayType { get; set; } = "Monthly";   // Monthly ya PerPiece
    public decimal MonthlySalary { get; set; }
    public bool IsActive { get; set; } = true;
}