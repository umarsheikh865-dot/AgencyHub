using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using AgencyHub.Application.Interfaces;
using AgencyHub.Domain.Entities;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AgencyHub.Infrastructure.Authentication;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // ============================================================
    // GENERATE ACCESS TOKEN
    // ============================================================

    public string GenerateAccessToken(User user)
    {
        // --------------------------------------------------------
        // Read JWT settings from appsettings.json
        // --------------------------------------------------------

        var key =
            _configuration["JwtSettings:Key"];

        var issuer =
            _configuration["JwtSettings:Issuer"];

        var audience =
            _configuration["JwtSettings:Audience"];

        // --------------------------------------------------------
        // Validate configuration
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "JWT Key is missing from configuration.");
        }

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException(
                "JWT Issuer is missing from configuration.");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "JWT Audience is missing from configuration.");
        }

        // --------------------------------------------------------
        // Create claims
        // --------------------------------------------------------

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                "userId",
                user.Id.ToString()),

            new(
                "tenantId",
                user.TenantId.ToString()),

            new(
                "email",
                user.Email),

            new(
                ClaimTypes.Email,
                user.Email),

            new(
                ClaimTypes.Role,
                user.Role.Name),

            new(
                "role",
                user.Role.Name)
        };

        // --------------------------------------------------------
        // Create signing key
        // --------------------------------------------------------

        var securityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

        // --------------------------------------------------------
        // Create signing credentials
        // --------------------------------------------------------

        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

        // --------------------------------------------------------
        // Token expiration
        // --------------------------------------------------------

        var expiration =
            DateTime.UtcNow.AddMinutes(
                GetAccessTokenExpirationMinutes());

        // --------------------------------------------------------
        // Create JWT
        // --------------------------------------------------------

        var token =
            new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiration,
                signingCredentials: credentials);

        // --------------------------------------------------------
        // Convert JWT to string
        // --------------------------------------------------------

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    // ============================================================
    // GET ACCESS TOKEN EXPIRATION
    // ============================================================

    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(
            GetAccessTokenExpirationMinutes());
    }

    // ============================================================
    // GENERATE REFRESH TOKEN
    // ============================================================

    public string GenerateRefreshToken()
    {
        var randomBytes =
            RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(
            randomBytes);
    }

    // ============================================================
    // GET ACCESS TOKEN EXPIRATION MINUTES
    // ============================================================

    private int GetAccessTokenExpirationMinutes()
    {
        var value =
            _configuration[
                "JwtSettings:AccessTokenExpirationMinutes"];

        return int.TryParse(
            value,
            out var minutes)
                ? minutes
                : 60;
    }
}