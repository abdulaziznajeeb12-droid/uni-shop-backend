using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class SalesController(ShopDbContext db) : ControllerBase
{
    // Tareekh se tareekh tak ke bills
    [HttpGet]
    public async Task<List<Sale>> Get(DateTime? from, DateTime? to)
    {
        var f = (from ?? DateTime.Today.AddDays(-29)).Date;
        var t = (to ?? DateTime.Today).Date.AddDays(1);
        return await db.Sales.Include(s => s.Items).Where(s => s.SaleDate >= f && s.SaleDate < t)
            .OrderByDescending(s => s.Id).ToListAsync();
    }

    [HttpPost]
    public async Task<IActionResult> Post(SaleReq r)
    {
        if (r.Items.Count == 0) return BadRequest("Bill khali hai");
        var pay = r.Payment ?? "Cash";
        if (pay == "Udhaar" && string.IsNullOrWhiteSpace(r.Customer)) return BadRequest("Udhaar ke liye customer ka naam likhein");
        await using var tx = await db.Database.BeginTransactionAsync();
        var ids = r.Items.Select(i => i.ProductId).ToList();
        var ps = await db.Products.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        var s = new Sale { Customer = r.Customer?.Trim(), Payment = pay };
        decimal sub = 0, cost = 0;
        foreach (var i in r.Items)
        {
            if (i.Qty < 1 || !ps.TryGetValue(i.ProductId, out var p)) return BadRequest("Product sahi nahi");
            if (p.Qty < i.Qty) return BadRequest($"Stock kam hai: {p.Name}");
            p.Qty -= i.Qty; sub += p.Price * i.Qty; cost += p.Cost * i.Qty;
            s.Items.Add(new SaleItem { ProductId = p.Id, Name = p.Name, Qty = i.Qty, Price = p.Price, Cost = p.Cost });
        }
        s.Discount = Math.Min(r.Discount, sub); s.Total = sub - s.Discount; s.Profit = s.Total - cost;
        db.Sales.Add(s); await db.SaveChangesAsync(); await tx.CommitAsync();
        return Ok(s);
    }
}