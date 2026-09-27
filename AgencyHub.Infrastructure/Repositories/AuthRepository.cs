using AgencyHub.Application.Interfaces;
using AgencyHub.Domain.Entities;
using AgencyHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyHub.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _context;

    public AuthRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetTenantBySlugAsync(string slug)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return await _context.Tenants
            .FirstOrDefaultAsync(x => x.Slug == normalizedSlug && x.IsActive);
    }

    public async Task<User?> GetUserByEmailAsync(Guid tenantId, string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.Users
            .Include(x => x.Role)
            .Include(x => x.Tenant)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Email == normalizedEmail && x.IsActive);
    }

    public async Task<Role?> GetRoleByNameAsync(string name)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(x => x.Name == name && x.IsActive);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(x => x.User).ThenInclude(x => x.Role)
            .Include(x => x.User).ThenInclude(x => x.Tenant)
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task<bool> TenantSlugExistsAsync(string slug)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return await _context.Tenants.AnyAsync(x => x.Slug == normalizedSlug);
    }

    public async Task AddTenantAsync(Tenant tenant)
    {
        await _context.Tenants.AddAsync(tenant);
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}