using Microsoft.EntityFrameworkCore;

namespace Core.Models
{
    public class TenantDbContext : DbContext
    {
        public DbSet<Tenant> Tenants { get; set; }

        public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tenant>().HasKey(x => x.Id);
            modelBuilder.Entity<Tenant>().Property(x => x.Id).ValueGeneratedOnAdd();
        }
    }
}