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

    public JwtTokenService(
        IConfiguration configuration)
    {
        _configuration =
            configuration;
    }

    public string GenerateAccessToken(
        User user)
    {
        var settings =
            _configuration.GetSection(
                "JwtSettings");

        var key =
            settings["Key"];

        var issuer =
            settings["Issuer"];

        var audience =
            settings["Audience"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "JWT Key is missing.");
        }

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException(
                "JWT Issuer is missing.");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "JWT Audience is missing.");
        }

        var claims =
            new List<Claim>
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

        var securityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    key));

        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

        var expiration =
            DateTime.UtcNow.AddMinutes(
                GetAccessTokenExpirationMinutes());

        var token =
            new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(
            GetAccessTokenExpirationMinutes());
    }

    public string GenerateRefreshToken()
    {
        var randomBytes =
            RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(
            randomBytes);
    }

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