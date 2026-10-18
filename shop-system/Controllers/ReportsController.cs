using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(ShopDbContext db) : ControllerBase
{
    // Khalis munafa = bill ka munafa - kharche
    [HttpGet("profit")]
    public async Task<IActionResult> Profit(DateTime? from, DateTime? to)
    {
        var f = (from ?? DateTime.Today.AddDays(-29)).Date;
        var t = (to ?? DateTime.Today).Date.AddDays(1);
        var sales = await db.Sales.Where(s => !s.IsVoid && s.SaleDate >= f && s.SaleDate < t).ToListAsync();
        var exps = await db.Expenses.Where(e => e.ExpenseDate >= f && e.ExpenseDate < t).ToListAsync();

        var days = sales.Select(s => s.SaleDate.Date).Concat(exps.Select(e => e.ExpenseDate.Date))
            .Distinct().OrderByDescending(d => d)
            .Select(d => new
            {
                date = d.ToString("yyyy-MM-dd"),
                sales = sales.Where(s => s.SaleDate.Date == d).Sum(s => s.Total),
                profit = sales.Where(s => s.SaleDate.Date == d).Sum(s => s.Profit),
                expenses = exps.Where(e => e.ExpenseDate.Date == d).Sum(e => e.Amount)
            }).ToList();

        var gross = sales.Sum(s => s.Profit);
        var spent = exps.Sum(e => e.Amount);
        return Ok(new
        {
            bills = sales.Count,
            salesTotal = sales.Sum(s => s.Total),
            grossProfit = gross,
            expenses = spent,
            netProfit = gross - spent,
            days
        });
    }
}