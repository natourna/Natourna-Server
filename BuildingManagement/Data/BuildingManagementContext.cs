using BuildingManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagement.Data
{
    public class BuildingManagementContext : DbContext
    {
        public DbSet<ApartementEntity> Apartements { get; set; }

        public DbSet<BillEntity> Bills { get; set; }

        public DbSet<BuildingEntity> Buildings { get; set; }

        public DbSet<CompoundEntity> Compounds { get; set; }

        public DbSet<PaymentEntity> Payments { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        public BuildingManagementContext(DbContextOptions<BuildingManagementContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
