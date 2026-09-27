using AgencyHub.Application.Interfaces;

using AgencyHub.Infrastructure.Authentication;
using AgencyHub.Infrastructure.Persistence;
using AgencyHub.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgencyHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "DefaultConnection");

        if (string.IsNullOrWhiteSpace(
            connectionString))
        {
            throw new InvalidOperationException(
                "DefaultConnection connection string is missing.");
        }

        services.AddDbContext<ApplicationDbContext>(
            options =>
            {
                options.UseSqlServer(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay:
                                TimeSpan.FromSeconds(10),
                            errorNumbersToAdd:
                                null);
                    });
            });

        // Authentication repository
        services.AddScoped<
            IAuthRepository,
            AuthRepository>();

        // Password hashing
        services.AddScoped<
            IPasswordHasher,
            PasswordHasher>();

        // JWT
        services.AddScoped<
            IJwtTokenService,
            JwtTokenService>();

        return services;
    }
}