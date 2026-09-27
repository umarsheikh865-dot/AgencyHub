using AgencyHub.Application.DTOs.Authentication;
using AgencyHub.Application.Interfaces;
using AgencyHub.Domain.Entities;

namespace AgencyHub.Application.Services;

public class AuthenticationService : IAuthService
{
    private readonly IAuthRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthenticationService(IAuthRepository repository, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        ValidateRegistrationRequest(request);

        var slug = request.AgencySlug.Trim().ToLowerInvariant();
        var email = request.Email.Trim().ToLowerInvariant();
        var agencyEmail = request.AgencyEmail.Trim().ToLowerInvariant();

        if (await _repository.TenantSlugExistsAsync(slug))
        {
            throw new InvalidOperationException("Agency slug is already registered.");
        }

        var existingTenant = await _repository.GetTenantBySlugAsync(slug);
        if (existingTenant != null)
        {
            throw new InvalidOperationException("Agency already exists.");
        }

        var adminRole = await _repository.GetRoleByNameAsync("AgencyAdmin");
        if (adminRole == null)
        {
            throw new InvalidOperationException("AgencyAdmin role was not found.");
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.AgencyName.Trim(),
            Slug = slug,
            Email = agencyEmail,
            Phone = request.Phone?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            RoleId = adminRole.Id,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Phone = request.Phone?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Tenant = tenant,
            Role = adminRole
        };

        await _repository.AddTenantAsync(tenant);
        await _repository.AddUserAsync(user);

        var refreshToken = CreateRefreshToken(user);
        await _repository.AddRefreshTokenAsync(refreshToken);
        await _repository.SaveChangesAsync();

        return CreateAuthResponse(user, refreshToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AgencySlug)) throw new ArgumentException("Agency slug is required.");
        if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Password is required.");

        var tenant = await _repository.GetTenantBySlugAsync(request.AgencySlug);
        if (tenant == null) throw new UnauthorizedAccessException("Invalid credentials.");

        var user = await _repository.GetUserByEmailAsync(tenant.Id, request.Email);
        if (user == null) throw new UnauthorizedAccessException("Invalid credentials.");

        var passwordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!passwordValid) throw new UnauthorizedAccessException("Invalid credentials.");

        var refreshToken = CreateRefreshToken(user);
        await _repository.AddRefreshTokenAsync(refreshToken);
        await _repository.SaveChangesAsync();

        return CreateAuthResponse(user, refreshToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken)) throw new ArgumentException("Refresh token is required.");

        var existingToken = await _repository.GetRefreshTokenAsync(request.RefreshToken);
        if (existingToken == null || !existingToken.IsActive) throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        existingToken.RevokedAt = DateTime.UtcNow;

        var newRefreshToken = CreateRefreshToken(existingToken.User);
        await _repository.AddRefreshTokenAsync(newRefreshToken);
        await _repository.SaveChangesAsync();

        return CreateAuthResponse(existingToken.User, newRefreshToken);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) throw new ArgumentException("Refresh token is required.");

        var existingToken = await _repository.GetRefreshTokenAsync(refreshToken);
        if (existingToken == null) return;

        if (!existingToken.IsRevoked)
        {
            existingToken.RevokedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync();
        }
    }

    private RefreshToken CreateRefreshToken(User user)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = _jwtTokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            User = user
        };
    }

    private AuthResponse CreateAuthResponse(User user, RefreshToken refreshToken)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = _jwtTokenService.GetAccessTokenExpiration(),
            UserId = user.Id,
            TenantId = user.TenantId,
            TenantName = user.Tenant.Name,
            Role = user.Role.Name,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }

    private static void ValidateRegistrationRequest(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AgencyName)) throw new ArgumentException("Agency name is required.");
        if (string.IsNullOrWhiteSpace(request.AgencySlug)) throw new ArgumentException("Agency slug is required.");
        if (string.IsNullOrWhiteSpace(request.AgencyEmail)) throw new ArgumentException("Agency email is required.");
        if (string.IsNullOrWhiteSpace(request.FirstName)) throw new ArgumentException("First name is required.");
        if (string.IsNullOrWhiteSpace(request.LastName)) throw new ArgumentException("Last name is required.");
        if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8) throw new ArgumentException("Password must contain at least 8 characters.");
    }
}