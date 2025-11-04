using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;

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

    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<PurchaseItem> PurchaseItems { get; set; }
    public DbSet<InventoryAdjustment> InventoryAdjustments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Username).HasColumnName("username").IsRequired();
            entity.Property(e => e.Password).HasColumnName("password").IsRequired();
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.Role)
                .HasColumnName("role")
                .HasConversion(
                    v => v.ToString().ToLowerInvariant(),
                    v => Enum.Parse<UserRole>(v, true)
                );
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion(
                    v => v.ToString().ToLowerInvariant(), // Lưu vào DB dưới dạng 'active', 'inactive', 'deleted'
                    v => Enum.Parse<EntityStatus>(v, true) // Đọc từ DB
                )
                .HasDefaultValue(EntityStatus.Active); // Giá trị mặc định
                                                       // Tự động thêm điều kiện WHERE Status != 'deleted' vào MỌI câu lệnh truy vấn
            entity.HasQueryFilter(u => u.Status != EntityStatus.Deleted);
        });


        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName).HasColumnName("category_name").IsRequired();
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion(
                    v => v.ToString().ToLowerInvariant(),
                    v => Enum.Parse<EntityStatus>(v, true)
                )
                .HasDefaultValue(EntityStatus.Active);

            // Tự động lọc các bản ghi đã bị "xóa mềm"
            entity.HasQueryFilter(c => c.Status != EntityStatus.Deleted);
        });

        // Configure Supplier entity
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion(
                    v => v.ToString().ToLowerInvariant(),
                    v => Enum.Parse<EntityStatus>(v, true)
                )
                .HasDefaultValue(EntityStatus.Active);

            // Tự động lọc các bản ghi đã bị "xóa mềm"
            entity.HasQueryFilter(s => s.Status != EntityStatus.Deleted);
        });

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.ProductName).HasColumnName("product_name").IsRequired();
            entity.Property(e => e.Barcode).HasColumnName("barcode");
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Unit).HasColumnName("unit").HasDefaultValue("pcs");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ImagePath).HasColumnName("image_path");
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion(
                    v => v.ToString().ToLowerInvariant(),
                    v => Enum.Parse<EntityStatus>(v, true)
                )
                .HasDefaultValue(EntityStatus.Active);

            // Tự động lọc các bản ghi đã bị "xóa mềm"
            entity.HasQueryFilter(p => p.Status != EntityStatus.Deleted);
            // Foreign keys
            entity.HasOne(e => e.Category)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Supplier)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Inventory entity
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("inventory");
            entity.Property(e => e.InventoryId).HasColumnName("inventory_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            // Foreign key to Product
            entity.HasOne(e => e.Product)
                .WithMany(e => e.Inventory)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add matching query filter to work with Product's global query filter
            entity.HasQueryFilter(i => i.Product!.Status != EntityStatus.Deleted);
        });

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PromoId).HasColumnName("promo_id");
            entity.Property(e => e.OrderDate).HasColumnName("order_date").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(10,2)");
            entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(10,2)");

            // Foreign keys
            entity.HasOne(e => e.Customer)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.User)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Promotion)
                .WithMany(p => p.Orders)
                .HasForeignKey(e => e.PromoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure OrderItem entity (NEW: Add mapping for OrderId, ProductId)
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");  // Map OrderId to order_id
            entity.Property(e => e.ProductId).HasColumnName("product_id");  // Map ProductId to product_id
            entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(10,2)").IsRequired();

            // Foreign keys
            entity.HasOne(e => e.Order)
                .WithMany(e => e.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(e => e.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent delete product if has orders
        });

        // Configure Promotion entity
        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.ToTable("promotions");
            entity.Property(e => e.PromoId).HasColumnName("promo_id");
            entity.Property(e => e.PromoCode).HasColumnName("promo_code");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountType).HasColumnName("discount_type").HasConversion<string>();
            entity.Property(e => e.DiscountValue).HasColumnName("discount_value").HasColumnType("decimal(10,2)");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.MinOrderAmount).HasColumnName("min_order_amount").HasColumnType("decimal(10,2)");
            entity.Property(e => e.UsageLimit).HasColumnName("usage_limit");
            entity.Property(e => e.UsedCount).HasColumnName("used_count");
            entity.Property(e => e.Status).HasColumnName("status");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasConversion<string>();
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign key
            entity.HasOne(e => e.Order)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion(
                    v => v.ToString().ToLowerInvariant(),
                    v => Enum.Parse<EntityStatus>(v, true)
                )
                .HasDefaultValue(EntityStatus.Active);

            // Tự động lọc các bản ghi đã bị "xóa mềm"
            entity.HasQueryFilter(c => c.Status != EntityStatus.Deleted);
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.ToTable("purchases");
            entity.HasKey(e => e.PurchaseId);
            entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            // Foreign keys
            entity.HasOne(e => e.Supplier)
                .WithMany() // Giả định Supplier không cần nav tới Purchases
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.User)
                .WithMany() // Giả định User không cần nav tới Purchases
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PurchaseItem>(entity =>
        {
            entity.ToTable("purchase_items");
            entity.HasKey(e => e.PurchaseItemId);
            entity.Property(e => e.PurchaseItemId).HasColumnName("purchase_item_id");
            entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.PurchasePrice).HasColumnName("purchase_price").HasColumnType("decimal(10,2)");

            // Cấu hình cột Subtotal như một cột được TÍNH TOÁN (như trong SQL của bạn)
            entity.Property(e => e.Subtotal)
                .HasColumnName("subtotal")
                .HasColumnType("decimal(10,2)")
                .ValueGeneratedOnAddOrUpdate() // Chỉ định đây là cột được tính toán
                .HasComputedColumnSql("(`quantity` * `purchase_price`)", stored: true); // SQL logic

            // Foreign keys
            entity.HasOne(e => e.Purchase)
                .WithMany(p => p.PurchaseItems) // Liên kết với collection trong Purchase
                .HasForeignKey(e => e.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa items nếu xóa purchase

            entity.HasOne(e => e.Product)
                .WithMany() // Giả định Product không cần nav tới PurchaseItems
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa Product nếu có lịch sử nhập
        });
        
        modelBuilder.Entity<InventoryAdjustment>(entity =>
        {
            entity.ToTable("inventory_adjustments");
            entity.HasKey(e => e.AdjustmentId);
            entity.Property(e => e.AdjustmentId).HasColumnName("adjustment_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(255);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign keys
            entity.HasOne(e => e.Product)
                .WithMany() // Giả định Product không cần nav tới Adjustments
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany() // Giả định User không cần nav tới Adjustments
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure primary keys
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

        modelBuilder.Entity<Purchase>().HasKey(e => e.PurchaseId);
        modelBuilder.Entity<PurchaseItem>().HasKey(e => e.PurchaseItemId);
        modelBuilder.Entity<InventoryAdjustment>().HasKey(e => e.AdjustmentId);
    }
}