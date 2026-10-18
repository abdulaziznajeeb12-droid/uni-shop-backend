
public record SaleReq(string? Customer, string? Payment, decimal Discount, List<LineReq> Items);