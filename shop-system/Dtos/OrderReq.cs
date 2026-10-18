
public record OrderReq(string? OrderType, string Customer, string? Organization, string? Phone, string? Address, DateOnly? DueDate, decimal Advance, string? Note, List<OrderLine> Items);