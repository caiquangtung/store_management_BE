using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Infrastructure.Data;

public class StoreDbContext : DbContext
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Inventory> Inventory { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Order>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Promotion>()
            .Property(e => e.DiscountType)
            .HasConversion<string>();

        modelBuilder.Entity<Payment>()
            .Property(e => e.PaymentMethod)
            .HasConversion<string>();

        // Configure primary keys explicitly for Database First
        modelBuilder.Entity<User>().HasKey(e => e.UserId);
        modelBuilder.Entity<Customer>().HasKey(e => e.CustomerId);
        modelBuilder.Entity<Category>().HasKey(e => e.CategoryId);
        modelBuilder.Entity<Supplier>().HasKey(e => e.SupplierId);
        modelBuilder.Entity<Product>().HasKey(e => e.ProductId);
        modelBuilder.Entity<Inventory>().HasKey(e => e.InventoryId);
        modelBuilder.Entity<Promotion>().HasKey(e => e.PromoId);
        modelBuilder.Entity<Order>().HasKey(e => e.OrderId);
        modelBuilder.Entity<OrderItem>().HasKey(e => e.OrderItemId);
        modelBuilder.Entity<Payment>().HasKey(e => e.PaymentId);

        // Configure decimal precision for MySQL
        modelBuilder.Entity<Product>()
            .Property(e => e.Price)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Order>()
            .Property(e => e.TotalAmount)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Order>()
            .Property(e => e.DiscountAmount)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(e => e.Price)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(e => e.Subtotal)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Promotion>()
            .Property(e => e.DiscountValue)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Promotion>()
            .Property(e => e.MinOrderAmount)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Payment>()
            .Property(e => e.Amount)
            .HasColumnType("decimal(10,2)");

    }
}
