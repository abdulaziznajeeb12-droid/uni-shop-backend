public record RateReq(string Item, decimal Rate);
public record LogReq(DateOnly? Date, string Item, int Qty, string? Note);
public record LeaveReq(DateOnly Date, string Kind);
public record PayReq(decimal Amount, string? Kind, string? Note, DateOnly? Date);