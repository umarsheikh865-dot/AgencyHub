using AgencyHub.Domain.Entities;

namespace AgencyHub.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);

    DateTime GetAccessTokenExpiration();

    string GenerateRefreshToken();
}