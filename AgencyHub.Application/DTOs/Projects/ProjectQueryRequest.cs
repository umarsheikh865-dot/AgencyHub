namespace AgencyHub.Application.DTOs.Projects;

public class ProjectQueryRequest
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public Guid? ClientId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Added for dynamic sorting support
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}