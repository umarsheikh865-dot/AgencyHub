using AgencyHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgencyHub.Infrastructure.Persistence
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();

            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Role>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "SuperAdmin",
                        Description = "System administrator"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "AgencyAdmin",
                        Description = "Agency administrator"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Manager",
                        Description = "Agency manager"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Employee",
                        Description = "Agency employee"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Client",
                        Description = "Agency client"
                    }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }
        }
    }
}