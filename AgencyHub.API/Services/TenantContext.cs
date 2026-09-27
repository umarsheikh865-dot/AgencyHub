using System.Security.Claims;

using AgencyHub.Application.Interfaces;

namespace AgencyHub.API.Services;

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor
        _httpContextAccessor;

    public TenantContext(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor =
            httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var value =
                _httpContextAccessor
                    .HttpContext?
                    .User
                    .FindFirst("tenantId")
                    ?.Value;

            return Guid.TryParse(
                value,
                out var id)
                ? id
                : null;
        }
    }

    public Guid? UserId
    {
        get
        {
            var value =
                _httpContextAccessor
                    .HttpContext?
                    .User
                    .FindFirst("userId")
                    ?.Value;

            return Guid.TryParse(
                value,
                out var id)
                ? id
                : null;
        }
    }

    public string? Role =>
        _httpContextAccessor
            .HttpContext?
            .User
            .FindFirst(ClaimTypes.Role)
            ?.Value;

    public bool IsAuthenticated =>
        _httpContextAccessor
            .HttpContext?
            .User
            .Identity?
            .IsAuthenticated
        ?? false;
}