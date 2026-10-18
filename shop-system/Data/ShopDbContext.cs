using Microsoft.EntityFrameworkCore;

public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<StockIn> StockIns => Set<StockIn>();
    public DbSet<CustomerPayment> CustomerPayments => Set<CustomerPayment>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderHistory> OrderHistory => Set<OrderHistory>();

    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<Worker> Workers => Set<Worker>();
    public DbSet<PieceRate> PieceRates => Set<PieceRate>();
    public DbSet<WorkLog> WorkLogs => Set<WorkLog>();
    public DbSet<Leave> Leaves => Set<Leave>();
    public DbSet<WorkerPayment> WorkerPayments => Set<WorkerPayment>();
}