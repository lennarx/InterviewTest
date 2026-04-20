using DotNetExamApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNetExamApi.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Product> Products => Set<Product>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public IEnumerable<Order> GetAllOrders()
    {
        return Orders.Include(o => o.Items).ToList();
    }

    public IQueryable<Order> GetActiveOrdersQuery()
    {
        return Orders
            .Include(o => o.Items)
            .Where(o => o.Status != OrderStatus.Cancelled);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.CustomerName).IsRequired();
            entity.Property(o => o.CustomerEmail).IsRequired();
            entity.HasMany(o => o.Items)
                  .WithOne(i => i.Order)
                  .HasForeignKey(i => i.OrderId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.HasOne(i => i.Product)
                  .WithMany()
                  .HasForeignKey(i => i.ProductId);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired();
        });
    }
}
