using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(ShopDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var t = DateTime.Today; var m = new DateTime(t.Year, t.Month, 1);
        var today = await db.Sales.Where(s => !s.IsVoid && s.SaleDate >= t).ToListAsync();
        var month = await db.Sales.Where(s => !s.IsVoid && s.SaleDate >= m).ToListAsync();
        var todayExp = await db.Expenses.Where(e => e.ExpenseDate >= t).SumAsync(e => e.Amount);
        var monthExp = await db.Expenses.Where(e => e.ExpenseDate >= m).SumAsync(e => e.Amount);
        var monthProfit = month.Sum(s => s.Profit);
        return Ok(new
        {
            todaySale = today.Sum(s => s.Total),
            todayProfit = today.Sum(s => s.Profit),
            todayExpenses = todayExp,
            monthSale = month.Sum(s => s.Total),
            monthProfit,
            monthExpenses = monthExp,
            monthNet = monthProfit - monthExp,
            supplierDue = await db.StockIns.SumAsync(s => s.Qty * s.Cost - s.PaidNow),
            lowStock = await db.Products.Where(p => p.Qty <= p.MinQty).Select(p => new { p.Id, p.Name, p.Qty, p.MinQty }).ToListAsync(),
            pendingOrders = await db.Orders.CountAsync(o => o.Status != "Delivered" && o.Status != "Cancelled")
        });
    }
}