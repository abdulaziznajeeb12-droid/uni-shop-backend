public class CustomerPayment
{
    public int Id { get; set; }
    public string Customer { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime PayDate { get; set; } = DateTime.Now;
}