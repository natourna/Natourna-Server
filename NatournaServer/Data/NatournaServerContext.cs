using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Tenancy;
using NatournaServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NatournaServer.Data
{
    /// <summary>
    /// Database context for Natourna Server System.
    /// Manages all entities and their relationships for building, apartment, payment, and financial operations.
    /// Tenant isolation: every ITenantEntity carries an OrganizationId that is stamped on insert and
    /// enforced on reads through global query filters driven by the current request's "orgId" claim.
    /// </summary>
    public class NatournaServerContext : DbContext
    {
        /// <summary>
        /// Organization of the current request; null outside an authenticated request
        /// (login, health checks, startup seeding), in which case the query filters are permissive.
        /// Kept as a context field so EF parameterizes the filters per context instance.
        /// </summary>
        private readonly int? _tenantOrganizationId;

        public DbSet<OrganizationEntity> Organizations { get; set; }

        public DbSet<SubscriptionEntity> Subscriptions { get; set; }

        public DbSet<CompoundEntity> Compounds { get; set; }

        public DbSet<BuildingEntity> Buildings { get; set; }

        public DbSet<ApartmentEntity> Apartments { get; set; }

        public DbSet<BalanceEntity> Balances { get; set; }

        public DbSet<BillEntity> Bills { get; set; }

        public DbSet<PaymentEntity> Payments { get; set; }

        public DbSet<CycleEntity> Cycles { get; set; }

        public DbSet<PaymentAllocationEntity> PaymentAllocations { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<RoleEntity> Roles { get; set; }

        public DbSet<AuditEntity> Audits { get; set; }

        public NatournaServerContext(DbContextOptions<NatournaServerContext> options, ITenantContext tenantContext) : base(options)
        {
            _tenantOrganizationId = tenantContext.OrganizationId;
        }

        /// <summary>
        /// Stamps OrganizationId on newly added tenant entities from the current request.
        /// Throws when neither the entity nor the request carries an organization -
        /// that always indicates a code path that forgot to set it explicitly (e.g. seeding).
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            StampTenantOnAddedEntities();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            StampTenantOnAddedEntities();
            return base.SaveChanges();
        }

        private void StampTenantOnAddedEntities()
        {
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State != EntityState.Added || entry.Entity.OrganizationId != 0)
                {
                    continue;
                }

                if (_tenantOrganizationId == null)
                {
                    throw new CustomException(
                        "TENANT-01",
                        $"Cannot insert {entry.Entity.GetType().Name} without an organization: no tenant in scope and OrganizationId was not set explicitly.");
                }

                entry.Entity.OrganizationId = _tenantOrganizationId.Value;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // TENANCY: ORGANIZATION RELATIONSHIPS
            // ========================================

            // Organization -> Compounds (One-to-Many)
            modelBuilder.Entity<OrganizationEntity>()
                .HasMany(o => o.Compounds)
                .WithOne()
                .HasForeignKey(c => c.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Organization -> Users (One-to-Many)
            modelBuilder.Entity<OrganizationEntity>()
                .HasMany(o => o.Users)
                .WithOne()
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Organization -> Subscription (One-to-One)
            modelBuilder.Entity<OrganizationEntity>()
                .HasOne(o => o.Subscription)
                .WithOne(s => s.Organization)
                .HasForeignKey<SubscriptionEntity>(s => s.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Referential integrity for the remaining tenant tables (no navigation needed)
            modelBuilder.Entity<BuildingEntity>()
                .HasOne<OrganizationEntity>().WithMany()
                .HasForeignKey(b => b.OrganizationId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApartmentEntity>()
                .HasOne<OrganizationEntity>().WithMany()
                .HasForeignKey(a => a.OrganizationId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BalanceEntity>()
                .HasOne<OrganizationEntity>().WithMany()
                .HasForeignKey(bal => bal.OrganizationId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BillEntity>()
                .HasOne<OrganizationEntity>().WithMany()
                .HasForeignKey(b => b.OrganizationId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentEntity>()
                .HasOne<OrganizationEntity>().WithMany()
                .HasForeignKey(p => p.OrganizationId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentAllocationEntity>()
                .HasOne<OrganizationEntity>().WithMany()
                .HasForeignKey(pa => pa.OrganizationId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CycleEntity>()
                .HasOne<OrganizationEntity>().WithMany()
                .HasForeignKey(c => c.OrganizationId).OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // TENANCY: GLOBAL QUERY FILTERS
            // ========================================
            // Permissive when no tenant is in scope (login lookups, startup seeding);
            // every authorized request carries the orgId claim and is strictly scoped.

            modelBuilder.Entity<CompoundEntity>()
                .HasQueryFilter(c => _tenantOrganizationId == null || c.OrganizationId == _tenantOrganizationId);

            modelBuilder.Entity<BuildingEntity>()
                .HasQueryFilter(b => _tenantOrganizationId == null || b.OrganizationId == _tenantOrganizationId);

            modelBuilder.Entity<ApartmentEntity>()
                .HasQueryFilter(a => _tenantOrganizationId == null || a.OrganizationId == _tenantOrganizationId);

            modelBuilder.Entity<BalanceEntity>()
                .HasQueryFilter(bal => _tenantOrganizationId == null || bal.OrganizationId == _tenantOrganizationId);

            modelBuilder.Entity<BillEntity>()
                .HasQueryFilter(b => _tenantOrganizationId == null || b.OrganizationId == _tenantOrganizationId);

            modelBuilder.Entity<PaymentEntity>()
                .HasQueryFilter(p => _tenantOrganizationId == null || p.OrganizationId == _tenantOrganizationId);

            modelBuilder.Entity<PaymentAllocationEntity>()
                .HasQueryFilter(pa => _tenantOrganizationId == null || pa.OrganizationId == _tenantOrganizationId);

            modelBuilder.Entity<CycleEntity>()
                .HasQueryFilter(c => _tenantOrganizationId == null || c.OrganizationId == _tenantOrganizationId);

            modelBuilder.Entity<UserEntity>()
                .HasQueryFilter(u => _tenantOrganizationId == null || u.OrganizationId == _tenantOrganizationId);

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
            // USER RELATIONSHIPS
            // ========================================

            // Role -> Users (One-to-Many)
            modelBuilder.Entity<RoleEntity>()
                .HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // INDEXES FOR PERFORMANCE
            // ========================================

            // Tenancy indexes - every tenant table is filtered by OrganizationId on each query
            modelBuilder.Entity<CompoundEntity>().HasIndex(c => c.OrganizationId);
            modelBuilder.Entity<BuildingEntity>().HasIndex(b => b.OrganizationId);
            modelBuilder.Entity<ApartmentEntity>().HasIndex(a => a.OrganizationId);
            modelBuilder.Entity<BalanceEntity>().HasIndex(bal => bal.OrganizationId);
            modelBuilder.Entity<BillEntity>().HasIndex(b => b.OrganizationId);
            modelBuilder.Entity<PaymentEntity>().HasIndex(p => p.OrganizationId);
            modelBuilder.Entity<PaymentAllocationEntity>().HasIndex(pa => pa.OrganizationId);
            modelBuilder.Entity<CycleEntity>().HasIndex(c => c.OrganizationId);
            modelBuilder.Entity<UserEntity>().HasIndex(u => u.OrganizationId);
            modelBuilder.Entity<AuditEntity>().HasIndex(l => l.OrganizationId);

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

            // User indexes
            modelBuilder.Entity<UserEntity>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<UserEntity>()
                .HasIndex(u => u.RoleId);

            // Role indexes
            modelBuilder.Entity<RoleEntity>()
                .HasIndex(r => r.Name)
                .IsUnique();

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

            modelBuilder.Entity<OrganizationEntity>()
                .Property(o => o.LbpExchangeRate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SubscriptionEntity>()
                .Property(s => s.PricePerBuilding)
                .HasPrecision(18, 2);
        }
    }
}
