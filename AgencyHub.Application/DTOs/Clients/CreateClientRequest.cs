namespace AgencyHub.Application.DTOs.Clients;

public class CreateClientRequest
{
    public string CompanyName { get; set; } =
        string.Empty;

    public string ContactPerson { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Industry { get; set; }

    public string Status { get; set; } =
        "Active";

    public string? Notes { get; set; }
}
