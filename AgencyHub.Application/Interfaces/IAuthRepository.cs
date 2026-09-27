using AgencyHub.Domain.Entities;

namespace AgencyHub.Application.Interfaces;

public interface IAuthRepository
{
    Task<Tenant?> GetTenantBySlugAsync(string slug);

    Task<User?> GetUserByEmailAsync(
        Guid tenantId,
        string email);

    Task<Role?> GetRoleByNameAsync(string name);

    Task<RefreshToken?> GetRefreshTokenAsync(
        string token);

    Task<bool> TenantSlugExistsAsync(
        string slug);

    Task AddTenantAsync(Tenant tenant);

    Task AddUserAsync(User user);

    Task AddRefreshTokenAsync(
        RefreshToken refreshToken);

    Task SaveChangesAsync();
}