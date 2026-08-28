using NatournaServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NatournaServer.Data
{
    /// <summary>
    /// Database context for Natourna Server System.
    /// Manages all entities and their relationships for building, apartment, payment, and financial operations.
    /// </summary>
    public class NatournaServerContext : DbContext
    {

        public DbSet<CompoundEntity> Compounds { get; set; }

        public DbSet<BuildingEntity> Buildings { get; set; }

        public DbSet<ApartmentEntity> Apartments { get; set; }

        public DbSet<BalanceEntity> Balances { get; set; }

        public DbSet<BillEntity> Bills { get; set; }

        public DbSet<PaymentEntity> Payments { get; set; }

        public DbSet<CycleEntity> Cycles { get; set; }

        public DbSet<PaymentAllocationEntity> PaymentAllocations { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<AuditEntity> Audits { get; set; }

        public NatournaServerContext(DbContextOptions<NatournaServerContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // BUILDING STRUCTURE RELATIONSHIPS
            // ========================================

            // Compound -> Buildings (One-to-Many)
            modelBuilder.Entity<CompoundEntity>()
                .HasMany(c => c.Buildings)
                .WithOne(b => b.Compound)
                .HasForeignKey(b => b.CompoundId)
                .OnDelete(DeleteBehavior.Cascade);

            // Building -> Apartments (One-to-Many)
            modelBuilder.Entity<BuildingEntity>()
                .HasMany(b => b.Apartments)
                .WithOne(a => a.Building)
                .HasForeignKey(a => a.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Apartment -> Payments (One-to-Many)
            modelBuilder.Entity<ApartmentEntity>()
                .HasMany(a => a.Payments)
                .WithOne(p => p.Apartment)
                .HasForeignKey(p => p.ApartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // FINANCIAL RELATIONSHIPS
            // ========================================

            // Compound -> Balances (One-to-Many)
            modelBuilder.Entity<CompoundEntity>()
                .HasMany(c => c.Balances)
                .WithOne(bal => bal.Compound)
                .HasForeignKey(bal => bal.CompoundId)
                .OnDelete(DeleteBehavior.Cascade);

            // Balance -> Bills (One-to-Many)
            modelBuilder.Entity<BalanceEntity>()
                .HasMany(bal => bal.Bills)
                .WithOne(bill => bill.Balance)
                .HasForeignKey(bill => bill.BalanceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Balance -> PaymentAllocations (One-to-Many)
            modelBuilder.Entity<BalanceEntity>()
                .HasMany(bal => bal.PaymentAllocations)
                .WithOne(pa => pa.Balance)
                .HasForeignKey(pa => pa.BalanceId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // PAYMENT ALLOCATION RELATIONSHIPS
            // ========================================

            // Payment -> PaymentAllocations (One-to-Many)
            // A payment can be split across multiple balances
            modelBuilder.Entity<PaymentEntity>()
                .HasMany(p => p.PaymentAllocations)
                .WithOne(pa => pa.Payment)
                .HasForeignKey(pa => pa.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // PaymentAllocation is a junction table linking Payment and Balance
            // with additional properties (Percentage, AllocatedAmount)
            modelBuilder.Entity<PaymentAllocationEntity>()
                .HasKey(pa => pa.Id);

            // ========================================
            // CYCLE RELATIONSHIPS
            // ========================================

            // Cycle -> Payments (One-to-Many)
            // A cycle generates multiple payments across apartments and time periods
            modelBuilder.Entity<CycleEntity>()
                .HasMany(c => c.Payments)
                .WithOne(p => p.Cycle)
                .HasForeignKey(p => p.CycleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // INDEXES FOR PERFORMANCE
            // ========================================

            // Compound indexes
            modelBuilder.Entity<CompoundEntity>()
                .HasIndex(c => c.Name);

            // Building indexes
            modelBuilder.Entity<BuildingEntity>()
                .HasIndex(b => b.CompoundId);

            // Apartment indexes
            modelBuilder.Entity<ApartmentEntity>()
                .HasIndex(a => a.BuildingId);

            modelBuilder.Entity<ApartmentEntity>()
                .HasIndex(a => a.IsActive);

            // Payment indexes
            modelBuilder.Entity<PaymentEntity>()
                .HasIndex(p => p.ApartmentId);

            modelBuilder.Entity<PaymentEntity>()
                .HasIndex(p => p.CycleId);

            modelBuilder.Entity<PaymentEntity>()
                .HasIndex(p => p.IsPaid);

            modelBuilder.Entity<PaymentEntity>()
                .HasIndex(p => p.DueDate);

            // Bill indexes
            modelBuilder.Entity<BillEntity>()
                .HasIndex(b => b.BalanceId);

            modelBuilder.Entity<BillEntity>()
                .HasIndex(b => b.IsPaid);

            // Balance indexes
            modelBuilder.Entity<BalanceEntity>()
                .HasIndex(bal => bal.CompoundId);

            // PaymentAllocation indexes
            modelBuilder.Entity<PaymentAllocationEntity>()
                .HasIndex(pa => pa.PaymentId);

            modelBuilder.Entity<PaymentAllocationEntity>()
                .HasIndex(pa => pa.BalanceId);

            // Cycle indexes
            modelBuilder.Entity<CycleEntity>()
                .HasIndex(c => c.IsActive);

            modelBuilder.Entity<CycleEntity>()
                .HasIndex(c => c.StartDate);

            modelBuilder.Entity<CycleEntity>()
                .HasIndex(c => c.EndDate);

            // Audit indexes
            modelBuilder.Entity<AuditEntity>()
                .HasIndex(l => l.UserId);

            modelBuilder.Entity<AuditEntity>()
                .HasIndex(l => l.UserEmail);

            modelBuilder.Entity<AuditEntity>()
                .HasIndex(l => l.Action);

            modelBuilder.Entity<AuditEntity>()
                .HasIndex(l => l.EntityType);

            modelBuilder.Entity<AuditEntity>()
                .HasIndex(l => l.CreatedAt);

            // ========================================
            // DECIMAL PRECISION CONFIGURATION
            // ========================================

            // Configure decimal precision for financial amounts
            modelBuilder.Entity<PaymentEntity>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PaymentAllocationEntity>()
                .Property(pa => pa.AllocatedAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PaymentAllocationEntity>()
                .Property(pa => pa.Percentage)
                .HasPrecision(5, 2);

            modelBuilder.Entity<BillEntity>()
                .Property(b => b.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BalanceEntity>()
                .Property(bal => bal.CurrentAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CycleEntity>()
                .Property(c => c.Amount)
                .HasPrecision(18, 2);
        }
    }
}
