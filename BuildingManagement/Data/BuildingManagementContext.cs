using BuildingManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagement.Data
{
    public class BuildingManagementContext : DbContext
    {
        public DbSet<ApartmentEntity> Apartments { get; set; }

        public DbSet<BillEntity> Bills { get; set; }

        public DbSet<BuildingEntity> Buildings { get; set; }

        public DbSet<CompoundEntity> Compounds { get; set; }

        public DbSet<PaymentEntity> Payments { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<CycleEntity> Cycles { get; set; }

        public DbSet<BalanceEntity> Balances { get; set; }

        public DbSet<PaymentAllocationEntity> PaymentAllocations { get; set; }

        public BuildingManagementContext(DbContextOptions<BuildingManagementContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PaymentAllocation many-to-many relationship
            modelBuilder.Entity<PaymentAllocationEntity>()
                .HasOne(pa => pa.Payment)
                .WithMany(p => p.PaymentAllocations)
                .HasForeignKey(pa => pa.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentAllocationEntity>()
                .HasOne(pa => pa.Balance)
                .WithMany(b => b.PaymentAllocations)
                .HasForeignKey(pa => pa.BalanceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Bill-Balance relationship (one-to-many)
            modelBuilder.Entity<BillEntity>()
                .HasOne(b => b.Balance)
                .WithMany(bal => bal.Bills)
                .HasForeignKey(b => b.BalanceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
