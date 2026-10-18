using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ShopDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<Product>> Get() => await db.Products.OrderBy(p => p.Name).ToListAsync();

    [HttpPost]
    public async Task<IActionResult> Post(Product p)
    {
        p.Id = 0; db.Products.Add(p); await db.SaveChangesAsync(); return Ok(p);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Product n)
    {
        var p = await db.Products.FindAsync(id); if (p == null) return NotFound();
        p.Name = n.Name; p.Sku = n.Sku; p.Category = n.Category; p.Cost = n.Cost; p.Price = n.Price;
        p.Qty = n.Qty; p.MinQty = n.MinQty; p.ExpiryDate = n.ExpiryDate;
        await db.SaveChangesAsync(); return Ok(p);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Products.FindAsync(id); if (p == null) return NotFound();
        db.Products.Remove(p); await db.SaveChangesAsync(); return NoContent();
    }
}