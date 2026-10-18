public class Expense
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.Now;
}