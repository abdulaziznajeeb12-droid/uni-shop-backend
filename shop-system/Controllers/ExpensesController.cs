using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController(ShopDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<Expense>> Get(DateTime? from, DateTime? to)
    {
        var f = (from ?? DateTime.Today.AddDays(-29)).Date;
        var t = (to ?? DateTime.Today).Date.AddDays(1);
        return await db.Expenses.Where(e => e.ExpenseDate >= f && e.ExpenseDate < t)
            .OrderByDescending(e => e.ExpenseDate).ToListAsync();
    }

    [HttpPost]
    public async Task<IActionResult> Post(Expense e)
    {
        if (string.IsNullOrWhiteSpace(e.Title) || e.Amount <= 0) return BadRequest("Detail aur raqam likhein");
        e.Id = 0;
        if (e.ExpenseDate == default) e.ExpenseDate = DateTime.Now;
        db.Expenses.Add(e); await db.SaveChangesAsync(); return Ok(e);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await db.Expenses.FindAsync(id); if (e == null) return NotFound();
        db.Expenses.Remove(e); await db.SaveChangesAsync(); return NoContent();
    }
}