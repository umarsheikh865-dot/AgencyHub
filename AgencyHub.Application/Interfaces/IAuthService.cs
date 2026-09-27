using AgencyHub.Application.DTOs.Authentication;

namespace AgencyHub.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request);

    Task<AuthResponse> LoginAsync(
        LoginRequest request);

    Task<AuthResponse> RefreshTokenAsync(
        RefreshTokenRequest request);

    Task LogoutAsync(
        string refreshToken);
}