using AgencyHub.Domain.Common;
using AgencyHub.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AgencyHub.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        private readonly string _tenantId;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _tenantId = httpContextAccessor.HttpContext?.Items["TenantId"]?.ToString() ?? "default-tenant";
        }

        public DbSet<Client> Clients => Set<Client>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global Query Filter: Automatically filters data so Tenant A never sees Tenant B's data
            modelBuilder.Entity<Client>().HasQueryFilter(c => c.TenantId == _tenantId);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.TenantId = _tenantId; // Automatically stamp record with current TenantId
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}