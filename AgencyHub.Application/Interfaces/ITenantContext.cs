namespace AgencyHub.Application.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }

    Guid? UserId { get; }

    string? Role { get; }

    bool IsAuthenticated { get; }
}