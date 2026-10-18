using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(ShopDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<Order>> Get() =>
        await db.Orders.Include(o => o.Items).Include(o => o.History).OrderByDescending(o => o.Id).ToListAsync();

    [HttpPost]
    public async Task<IActionResult> Post(OrderReq r)
    {
        if (string.IsNullOrWhiteSpace(r.Customer) || r.Items.Count == 0) return BadRequest("Customer ka naam aur items zaroori hain");
        var type = r.OrderType == "Uniform" ? "Uniform" : "Normal";
        var o = new Order
        {
            OrderType = type,
            Customer = r.Customer.Trim(),
            Organization = r.Organization,
            Phone = r.Phone,
            Address = r.Address,
            DueDate = r.DueDate,
            Note = r.Note,
            Status = OrderStages.Map[type][0]
        };
        foreach (var i in r.Items)
        {
            if (i.Qty < 1) return BadRequest("Miqdar sahi nahi");
            var item = new OrderItem { Qty = i.Qty, SchoolName = i.SchoolName, StudentName = i.StudentName, Gender = i.Gender, Measurements = i.Measurements };
            if (i.ProductId is int pid)
            {
                var p = await db.Products.FindAsync(pid); if (p == null) return BadRequest("Product sahi nahi");
                item.ProductId = pid; item.Name = p.Name; item.Price = p.Price; item.Cost = p.Cost;
            }
            else { item.Name = i.Name ?? "Item"; item.Price = i.Price; item.Cost = i.Cost; }
            o.Items.Add(item);
        }
        o.Total = o.Items.Sum(i => i.Qty * i.Price); o.Advance = Math.Min(r.Advance, o.Total);
        o.History.Add(new OrderHistory { Status = o.Status });
        db.Orders.Add(o); await db.SaveChangesAsync(); return Ok(o);
    }
    [HttpPost("{id:int}/advance")]
    public async Task<IActionResult> Advance(int id, bool? paid, string? courier, string? tracking)
    {
        var o = await db.Orders.Include(x => x.Items).Include(x => x.History).FirstOrDefaultAsync(x => x.Id == id);
        if (o == null) return NotFound();
        var st = OrderStages.Map[o.OrderType]; var i = Array.IndexOf(st, o.Status);
        if (i < 0 || i >= st.Length - 1) return BadRequest("Order aage nahi barh sakta");
        var next = st[i + 1];
        if (next == "Rawana") { o.Courier = courier; o.Tracking = tracking; }
        if (next == "Delivered")
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            var stocked = o.Items.Where(x => x.ProductId != null).ToList();
            var ids = stocked.Select(x => x.ProductId!.Value).ToList();
            var ps = await db.Products.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
            foreach (var it in stocked)
            {
                var p = ps[it.ProductId!.Value];
                if (p.Qty < it.Qty) return BadRequest($"Stock kam hai: {p.Name}");
                p.Qty -= it.Qty;
            }
            var isPaid = o.Total - o.Advance <= 0 || (paid ?? true);
            db.Sales.Add(new Sale
            {
                Customer = o.Customer,
                Payment = isPaid ? "Cash" : "Udhaar",
                Total = o.Total,
                Profit = o.Total - o.Items.Sum(x => x.Qty * x.Cost),
                Items = o.Items.Select(x => new SaleItem { ProductId = x.ProductId, Name = x.Name, Qty = x.Qty, Price = x.Price, Cost = x.Cost }).ToList()
            });
            if (!isPaid && o.Advance > 0) db.CustomerPayments.Add(new CustomerPayment { Customer = o.Customer, Amount = o.Advance });
            o.Status = next; o.History.Add(new OrderHistory { Status = next });
            await db.SaveChangesAsync(); await tx.CommitAsync(); return Ok(o);
        }
        o.Status = next; o.History.Add(new OrderHistory { Status = next });
        await db.SaveChangesAsync(); return Ok(o);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var o = await db.Orders.Include(x => x.History).FirstOrDefaultAsync(x => x.Id == id);
        if (o == null || o.Status is "Delivered" or "Cancelled") return BadRequest("Order cancel nahi ho sakta");
        o.Status = "Cancelled"; o.History.Add(new OrderHistory { Status = "Cancelled" });
        await db.SaveChangesAsync(); return Ok(o);
    }
}