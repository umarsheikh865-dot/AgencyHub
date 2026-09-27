namespace AgencyHub.Application.DTOs.Authentication;

public class RegisterRequest
{
    public string AgencyName { get; set; } = string.Empty;

    public string AgencySlug { get; set; } = string.Empty;

    public string AgencyEmail { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? Phone { get; set; }
}