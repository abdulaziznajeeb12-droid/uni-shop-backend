public class Leave
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public DateOnly LeaveDate { get; set; }
    public string Kind { get; set; } = "Absent";   // Absent ya Half
}