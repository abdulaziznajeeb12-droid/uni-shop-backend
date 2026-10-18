using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class WorkersController(ShopDbContext db) : ControllerBase
{
    IActionResult Done() => Ok(new { ok = true });
    static DateOnly Today() => DateOnly.FromDateTime(DateTime.Today);

    // month = "2026-10" . Khali ho to is mahine ka hisaab
    static (DateOnly first, DateOnly last) Range(string? month)
    {
        var ok = DateTime.TryParse((month ?? "") + "-01", out var d);
        if (!ok) d = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var first = new DateOnly(d.Year, d.Month, 1);
        return (first, first.AddMonths(1).AddDays(-1));
    }

    static object Summary(Worker w, DateOnly last, IEnumerable<WorkLog> logs, IEnumerable<Leave> leaves, IEnumerable<WorkerPayment> pays)
    {
        var l = logs.Where(x => x.WorkerId == w.Id).ToList();
        decimal absent = leaves.Where(x => x.WorkerId == w.Id).Sum(x => x.Kind == "Half" ? 0.5m : 1m);
        var paid = pays.Where(x => x.WorkerId == w.Id).Sum(x => x.Amount);
        decimal earned = w.PayType == "Monthly"
            ? Math.Max(0, w.MonthlySalary - w.MonthlySalary / last.Day * absent)   // chutti ka katoti
            : l.Sum(x => x.Amount);                                                 // piece ki ujrat
        earned = Math.Round(earned);
        return new { w.Id, w.Name, w.Phone, w.PayType, w.MonthlySalary, earned, paid, balance = earned - paid, absentDays = absent, pieces = l.Sum(x => x.Qty) };
    }

    [HttpGet]
    public async Task<IActionResult> Get(string? month)
    {
        var (f, t) = Range(month);
        var ws = await db.Workers.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync();
        var logs = await db.WorkLogs.Where(x => x.WorkDate >= f && x.WorkDate <= t).ToListAsync();
        var leaves = await db.Leaves.Where(x => x.LeaveDate >= f && x.LeaveDate <= t).ToListAsync();
        var pays = await db.WorkerPayments.Where(x => x.PayDate >= f && x.PayDate <= t).ToListAsync();
        return Ok(ws.Select(w => Summary(w, t, logs, leaves, pays)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id, string? month)
    {
        var w = await db.Workers.FindAsync(id); if (w == null) return NotFound();
        var (f, t) = Range(month);
        var logs = await db.WorkLogs.Where(x => x.WorkerId == id && x.WorkDate >= f && x.WorkDate <= t)
            .OrderByDescending(x => x.WorkDate).ThenByDescending(x => x.Id).ToListAsync();
        var leaves = await db.Leaves.Where(x => x.WorkerId == id && x.LeaveDate >= f && x.LeaveDate <= t).OrderBy(x => x.LeaveDate).ToListAsync();
        var pays = await db.WorkerPayments.Where(x => x.WorkerId == id && x.PayDate >= f && x.PayDate <= t)
            .OrderByDescending(x => x.PayDate).ThenByDescending(x => x.Id).ToListAsync();
        var rates = await db.PieceRates.Where(x => x.WorkerId == id).OrderBy(x => x.Item).ToListAsync();
        return Ok(new { worker = w, summary = Summary(w, t, logs, leaves, pays), rates, logs, leaves, payments = pays });
    }

    [HttpPost]
    public async Task<IActionResult> Post(Worker w)
    {
        if (string.IsNullOrWhiteSpace(w.Name)) return BadRequest("Naam likhein");
        w.Id = 0; w.IsActive = true; w.PayType = w.PayType == "PerPiece" ? "PerPiece" : "Monthly";
        db.Workers.Add(w); await db.SaveChangesAsync(); return Ok(w);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Worker n)
    {
        var w = await db.Workers.FindAsync(id); if (w == null) return NotFound();
        w.Name = n.Name; w.Phone = n.Phone; w.MonthlySalary = n.MonthlySalary;
        w.PayType = n.PayType == "PerPiece" ? "PerPiece" : "Monthly";
        await db.SaveChangesAsync(); return Ok(w);
    }

    [HttpDelete("{id:int}")]   // list se hata deta hai, hisaab bacha rehta hai
    public async Task<IActionResult> Delete(int id)
    {
        var w = await db.Workers.FindAsync(id); if (w == null) return NotFound();
        w.IsActive = false; await db.SaveChangesAsync(); return NoContent();
    }

    // ---- Piece rates ----
    [HttpPost("{id:int}/rates")]
    public async Task<IActionResult> AddRate(int id, RateReq r)
    {
        if (string.IsNullOrWhiteSpace(r.Item) || r.Rate <= 0) return BadRequest("Item aur rate likhein");
        var item = r.Item.Trim();
        var x = await db.PieceRates.FirstOrDefaultAsync(p => p.WorkerId == id && p.Item == item);
        if (x == null) db.PieceRates.Add(new PieceRate { WorkerId = id, Item = item, Rate = r.Rate }); else x.Rate = r.Rate;
        await db.SaveChangesAsync(); return Done();
    }
    [HttpDelete("rates/{rid:int}")]
    public async Task<IActionResult> DelRate(int rid)
    {
        var x = await db.PieceRates.FindAsync(rid); if (x != null) { db.PieceRates.Remove(x); await db.SaveChangesAsync(); }
        return NoContent();
    }

    // ---- Roz ka kaam (rate server khud lagata hai) ----
    [HttpPost("{id:int}/logs")]
    public async Task<IActionResult> AddLog(int id, LogReq r)
    {
        if (r.Qty < 1) return BadRequest("Miqdar likhein");
        var item = (r.Item ?? "").Trim();
        var rate = await db.PieceRates.FirstOrDefaultAsync(p => p.WorkerId == id && p.Item == item);
        if (rate == null) return BadRequest("Is item ka rate pehle set karein");
        db.WorkLogs.Add(new WorkLog { WorkerId = id, WorkDate = r.Date ?? Today(), Item = item, Qty = r.Qty, Rate = rate.Rate, Amount = rate.Rate * r.Qty, Note = r.Note });
        await db.SaveChangesAsync(); return Done();
    }
    [HttpDelete("logs/{lid:int}")]
    public async Task<IActionResult> DelLog(int lid)
    {
        var x = await db.WorkLogs.FindAsync(lid); if (x != null) { db.WorkLogs.Remove(x); await db.SaveChangesAsync(); }
        return NoContent();
    }

    // ---- Chutti (mahana wale workers ke liye) ----
    [HttpPost("{id:int}/leaves")]
    public async Task<IActionResult> AddLeave(int id, LeaveReq r)
    {
        var kind = r.Kind == "Half" ? "Half" : "Absent";
        var x = await db.Leaves.FirstOrDefaultAsync(l => l.WorkerId == id && l.LeaveDate == r.Date);
        if (x == null) db.Leaves.Add(new Leave { WorkerId = id, LeaveDate = r.Date, Kind = kind }); else x.Kind = kind;
        await db.SaveChangesAsync(); return Done();
    }
    [HttpDelete("leaves/{lid:int}")]
    public async Task<IActionResult> DelLeave(int lid)
    {
        var x = await db.Leaves.FindAsync(lid); if (x != null) { db.Leaves.Remove(x); await db.SaveChangesAsync(); }
        return NoContent();
    }

    // ---- Payment (Kharche mein bhi chali jati hai) ----
    [HttpPost("{id:int}/payments")]
    public async Task<IActionResult> Pay(int id, PayReq r)
    {
        var w = await db.Workers.FindAsync(id); if (w == null) return NotFound();
        if (r.Amount <= 0) return BadRequest("Raqam likhein");
        var kind = r.Kind == "Advance" ? "Advance" : "Salary"; var date = r.Date ?? Today();
        var e = new Expense { Title = $"{w.Name} - {(kind == "Advance" ? "Peshgi" : "Tankhwah / ujrat")}", Amount = r.Amount, ExpenseDate = date.ToDateTime(TimeOnly.MinValue) };
        db.Expenses.Add(e); await db.SaveChangesAsync();
        db.WorkerPayments.Add(new WorkerPayment { WorkerId = id, PayDate = date, Amount = r.Amount, Kind = kind, Note = r.Note, ExpenseId = e.Id });
        await db.SaveChangesAsync(); return Done();
    }
    [HttpDelete("payments/{pid:int}")]
    public async Task<IActionResult> DelPay(int pid)
    {
        var p = await db.WorkerPayments.FindAsync(pid); if (p == null) return NoContent();
        if (p.ExpenseId is int eid) { var e = await db.Expenses.FindAsync(eid); if (e != null) db.Expenses.Remove(e); }
        db.WorkerPayments.Remove(p); await db.SaveChangesAsync(); return NoContent();
    }
}