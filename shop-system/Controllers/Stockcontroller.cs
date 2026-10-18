using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/stock-in")]
public class StockController(ShopDbContext db) : ControllerBase
{
    // Khareedari ka hisaab: kul maal, ada ki hui raqam, baqaya
    [HttpGet]
    public async Task<IActionResult> Get(DateTime? from, DateTime? to)
    {
        var f = (from ?? DateTime.Today.AddDays(-29)).Date;
        var t = (to ?? DateTime.Today).Date.AddDays(1);
        var rows = await db.StockIns.Where(s => s.StockDate >= f && s.StockDate < t)
            .Join(db.Products, s => s.ProductId, p => p.Id, (s, p) => new
            {
                s.Id,
                date = s.StockDate,
                product = p.Name,
                supplier = s.SupplierName,
                s.Qty,
                s.Cost,
                total = s.Qty * s.Cost,
                paid = s.PaidNow,
                due = s.Qty * s.Cost - s.PaidNow
            })
            .OrderByDescending(x => x.date).ToListAsync();
        return Ok(new { rows, totalPurchase = rows.Sum(x => x.total), totalPaid = rows.Sum(x => x.paid), totalDue = rows.Sum(x => x.due) });
    }

    [HttpPost]
    public async Task<IActionResult> Post(StockReq r)
    {
        var p = await db.Products.FindAsync(r.ProductId);
        if (p == null || r.Qty < 1) return BadRequest("Product ya miqdar sahi nahi");
        var newCost = r.Cost > 0 ? r.Cost : p.Cost;            // is maal ki asli qeemat
        // Average cost: purana + naya maal milake
        p.Cost = p.Qty > 0 ? Math.Round((p.Qty * p.Cost + r.Qty * newCost) / (p.Qty + r.Qty), 2) : newCost;
        p.Qty += r.Qty;
        var total = r.Qty * newCost;
        db.StockIns.Add(new StockIn
        {
            ProductId = p.Id,
            Qty = r.Qty,
            Cost = newCost,
            SupplierName = r.Supplier?.Trim(),
            PaidNow = Math.Min(Math.Max(r.PaidNow, 0), total)
        });
        await db.SaveChangesAsync(); return Ok(p);
    }


    // Baqaya raqam baad mein ada karna
    [HttpPost("{id:int}/pay")]
    public async Task<IActionResult> Pay(int id, StockPayReq r)
    {
        var s = await db.StockIns.FindAsync(id); if (s == null) return NotFound();
        var due = s.Qty * s.Cost - s.PaidNow;
        if (r.Amount <= 0 || r.Amount > due + 0.5m) return BadRequest("Raqam baqaya se zyada nahi ho sakti");
        s.PaidNow += Math.Min(r.Amount, due); await db.SaveChangesAsync(); return Ok(new { ok = true });
    }
}