using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Models.Entities;

namespace SistemaInventario.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Adjustment> Adjustments { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<ProductHistory> ProductHistories { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CashRegister> CashRegisters { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<InventoryCycle> InventoryCycles { get; set; }
        public DbSet<InventoryCount> InventoryCounts { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<SaleRefund> SaleRefunds { get; set; }
        public DbSet<SaleRefundDetail> SaleRefundDetails { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<ApplicationUser>()
                .Property(u => u.Salary)
                .HasPrecision(18, 2);

            // Configure Customer relationships
            builder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure CashRegister relationships
            builder.Entity<Sale>()
                .HasOne(s => s.CashRegister)
                .WithMany(cr => cr.Sales)
                .HasForeignKey(s => s.CashRegisterId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<CashRegister>()
                .HasOne(cr => cr.User)
                .WithMany()
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure decimal precision for CashRegister
            builder.Entity<CashRegister>()
                .Property(cr => cr.OpeningBalance)
                .HasPrecision(18, 2);

            builder.Entity<CashRegister>()
                .Property(cr => cr.ExpectedBalance)
                .HasPrecision(18, 2);

            builder.Entity<CashRegister>()
                .Property(cr => cr.ActualBalance)
                .HasPrecision(18, 2);

            builder.Entity<CashRegister>()
                .Property(cr => cr.Difference)
                .HasPrecision(18, 2);

            // Configure decimal precision for Sale payment fields
            builder.Entity<Sale>()
                .Property(s => s.AmountPaid)
                .HasPrecision(18, 2);

            builder.Entity<Sale>()
                .Property(s => s.Change)
                .HasPrecision(18, 2);

            // Configure decimal precision for Expenses
            builder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);

            // Configure SaleRefund relationships
            builder.Entity<SaleRefund>()
                .HasOne(sr => sr.Sale)
                .WithMany(s => s.Refunds)
                .HasForeignKey(sr => sr.SaleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SaleRefund>()
                .HasOne(sr => sr.User)
                .WithMany()
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<SaleRefund>()
                .HasOne(sr => sr.CashRegister)
                .WithMany()
                .HasForeignKey(sr => sr.CashRegisterId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure SaleRefundDetail relationships
            builder.Entity<SaleRefundDetail>()
                .HasOne(srd => srd.SaleRefund)
                .WithMany(sr => sr.RefundDetails)
                .HasForeignKey(srd => srd.SaleRefundId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SaleRefundDetail>()
                .HasOne(srd => srd.SaleDetail)
                .WithMany()
                .HasForeignKey(srd => srd.SaleDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SaleRefundDetail>()
                .HasOne(srd => srd.Product)
                .WithMany()
                .HasForeignKey(srd => srd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure decimal precision for Sale refund fields
            builder.Entity<Sale>()
                .Property(s => s.RefundedAmount)
                .HasPrecision(18, 2);

            // Configure decimal precision for SaleRefund
            builder.Entity<SaleRefund>()
                .Property(sr => sr.RefundAmount)
                .HasPrecision(18, 2);

            // Configure decimal precision for SaleRefundDetail
            builder.Entity<SaleRefundDetail>()
                .Property(srd => srd.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<SaleRefundDetail>()
                .Property(srd => srd.Subtotal)
                .HasPrecision(18, 2);

        }
    }
}
