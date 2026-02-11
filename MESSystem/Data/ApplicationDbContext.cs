using Microsoft.EntityFrameworkCore;
using MESSystem.Models;

namespace MESSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<CardItem> CardItems { get; set; }
    public DbSet<EventLog> EventLogs { get; set; }
    
    // ERP 매출 관리
    public DbSet<SalesClosing> SalesClosings { get; set; }
    public DbSet<SalesClosingItem> SalesClosingItems { get; set; }
    public DbSet<TaxInvoice> TaxInvoices { get; set; }
    public DbSet<TaxInvoiceItem> TaxInvoiceItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<BankTransaction> BankTransactions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
        });
        
        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.CardOrder).IsUnique();
        });
        
        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Client
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
        });
        
        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.IsDeleted);
            
            entity.HasOne(e => e.Client)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.ParentOrder)
                  .WithMany(o => o.ChildOrders)
                  .HasForeignKey(e => e.ParentOrderId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Card
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasIndex(e => e.CardNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsModified);
            
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.Cards)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Cards)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // CardItem
        modelBuilder.Entity<CardItem>(entity =>
        {
            entity.HasOne(e => e.Card)
                  .WithMany(c => c.CardItems)
                  .HasForeignKey(e => e.CardId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.OrderItem)
                  .WithMany(oi => oi.CardItems)
                  .HasForeignKey(e => e.OrderItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // EventLog
        modelBuilder.Entity<EventLog>(entity =>
        {
            entity.HasIndex(e => e.CardNumber);
            entity.HasIndex(e => e.Timestamp).IsDescending();
            entity.HasIndex(e => e.IsProcessed);
            
            entity.HasOne(e => e.Card)
                  .WithMany(c => c.EventLogs)
                  .HasForeignKey(e => e.CardId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
        
        // SalesClosing
        modelBuilder.Entity<SalesClosing>(entity =>
        {
            entity.HasIndex(e => e.ClosingNumber).IsUnique();
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartDate);
            
            entity.Property(e => e.SupplyAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.AdditionalAmount).HasPrecision(18, 2);
            entity.Property(e => e.AdjustedSupplyAmount).HasPrecision(18, 2);
            entity.Property(e => e.VatAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Client)
                  .WithMany(c => c.SalesClosings)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // SalesClosingItem
        modelBuilder.Entity<SalesClosingItem>(entity =>
        {
            entity.HasIndex(e => e.SalesClosingId);
            entity.HasIndex(e => e.OrderId);
            
            entity.Property(e => e.SupplyAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            
            entity.HasOne(e => e.SalesClosing)
                  .WithMany(s => s.Items)
                  .HasForeignKey(e => e.SalesClosingId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Order)
                  .WithMany()
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // TaxInvoice
        modelBuilder.Entity<TaxInvoice>(entity =>
        {
            entity.HasIndex(e => e.SalesClosingId).IsUnique();
            entity.HasIndex(e => e.ApprovalNumber);
            entity.HasIndex(e => e.IssueDate);
            entity.HasIndex(e => e.Status);
            
            entity.Property(e => e.SupplyAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            
            entity.HasOne(e => e.SalesClosing)
                  .WithOne(s => s.TaxInvoice)
                  .HasForeignKey<TaxInvoice>(e => e.SalesClosingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // TaxInvoiceItem
        modelBuilder.Entity<TaxInvoiceItem>(entity =>
        {
            entity.HasIndex(e => e.TaxInvoiceId);
            
            entity.Property(e => e.Quantity).HasPrecision(18, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.SupplyAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            
            entity.HasOne(e => e.TaxInvoice)
                  .WithMany(t => t.Items)
                  .HasForeignKey(e => e.TaxInvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.PaymentNumber).IsUnique();
            entity.HasIndex(e => e.SalesClosingId);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.PaymentDate);
            entity.HasIndex(e => e.IsMatched);
            
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            
            entity.HasOne(e => e.SalesClosing)
                  .WithMany(s => s.Payments)
                  .HasForeignKey(e => e.SalesClosingId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.Client)
                  .WithMany(c => c.Payments)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.BankTransaction)
                  .WithMany(b => b.Payments)
                  .HasForeignKey(e => e.BankTransactionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
        
        // BankTransaction
        modelBuilder.Entity<BankTransaction>(entity =>
        {
            entity.HasIndex(e => e.ExternalTransactionId).IsUnique();
            entity.HasIndex(e => e.TransactionDateTime);
            entity.HasIndex(e => e.IsProcessed);
            
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Balance).HasPrecision(18, 2);
        });
        
        // 초기 데이터
        SeedData(modelBuilder);
    }
    
    private void SeedData(ModelBuilder modelBuilder)
    {
        // 초기 분류
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "태극기", CardOrder = 1, IsActive = true, CreatedAt = DateTime.Now },
            new Category { Id = 2, Name = "현수막", CardOrder = 2, IsActive = true, CreatedAt = DateTime.Now },
            new Category { Id = 3, Name = "간판", CardOrder = 3, IsActive = true, CreatedAt = DateTime.Now }
        );
        
        // 초기 사용자 (비밀번호: admin123, user123)
        // 실제 환경에서는 BCrypt 등으로 해시해야 함
        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = 1, 
                Username = "admin", 
                Password = "admin123", // TODO: 해시 필요
                FullName = "관리자", 
                Role = "관리자", 
                IsActive = true, 
                CreatedAt = DateTime.Now 
            },
            new User 
            { 
                Id = 2, 
                Username = "field01", 
                Password = "user123", // TODO: 해시 필요
                FullName = "현장작업자1", 
                Role = "사용자", 
                IsActive = true, 
                CreatedAt = DateTime.Now 
            }
        );
    }
}
