using Microsoft.EntityFrameworkCore;
using OrderService.Models;


namespace OrderService.Data;

// Remain connection, track in RAM, concurrently implement CRUD database via DbSet<T> correspond tables
public class OrderDbContext: DbContext // Like Gateway about code and database
{

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Promotion> Promotions { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mapping class and table
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.ToTable("promotions");
            entity.HasKey(e => e.PromoId);
            entity.Property(e => e.PromoId).HasColumnName("promo_id");
            entity.Property(e => e.PromoCode).HasColumnName("promo_code").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);
            entity.Property(e => e.DiscountType).HasColumnName("discount_type").HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.DiscountValue).HasColumnName("discount_value").HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.StartDate).HasColumnName("start_date").HasColumnType("date").IsRequired();
            entity.Property(e => e.EndDate).HasColumnName("end_date").HasColumnType("date").IsRequired();
            entity.Property(e => e.MinOrderAmount).HasColumnName("min_order_amount").HasColumnType("decimal(10,2)");
            entity.Property(e => e.UsageLimit).HasColumnName("usage_limit");
            entity.Property(e => e.UsedCount).HasColumnName("used_count");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);

            entity.HasIndex(e => e.PromoCode).IsUnique();
        });


        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PromoId).HasColumnName("promo_id");
            entity.Property(e => e.OrderDate).HasColumnName("order_date");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(10,2)");
            entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(10,2)");

            // Order Item
            entity.HasMany(e => e.OrderItems)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);


            // Payment
            entity.HasMany(e => e.Payments)
            .WithOne()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(e => e.OrderItemId);
            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(10,2)");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price").HasColumnType("decimal(10,2)");
        });



        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
            entity.Property(e => e.PaymentMethod)
            .HasColumnName("payment_method")
            .HasConversion(
                v => v.ToString().ToLower().Replace("ewallet", "e_wallet"),
                v => Enum.Parse<PaymentMethod>(v.Replace("e_wallet", "ewallet"), true)
            )
            .HasMaxLength(20);
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
        });

    }

}