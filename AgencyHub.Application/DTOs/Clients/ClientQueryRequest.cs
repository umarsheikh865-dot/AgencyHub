namespace AgencyHub.Application.DTOs.Clients;

public class ClientQueryRequest
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public string? Industry { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}