using Microsoft.EntityFrameworkCore;
using AuthService.Models;


namespace AuthService.Data;



public class AuthDbContext: DbContext
{
    public AuthDbContext(
        DbContextOptions<AuthDbContext> options
    ): base(options) {


        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity => {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Password).HasColumnName("password").HasMaxLength(255).IsRequired();
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100);
            entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.Username).IsUnique();
        });


        modelBuilder.Entity<Customer>(entity => {
            entity.ToTable("customers");
            entity.HasKey(e => e.CustomerId);
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.Address).HasColumnName("address").HasColumnType("text");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<RefreshToken>(entity => {
            entity.ToTable("refresh_token");
            entity.HasKey(e => e.TokenId);
            entity.Property(e => e.TokenId).HasColumnName("token_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

    }


}