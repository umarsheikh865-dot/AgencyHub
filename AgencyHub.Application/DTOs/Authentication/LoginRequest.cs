namespace AgencyHub.Application.DTOs.Authentication;

public class LoginRequest
{
    public string AgencySlug { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}