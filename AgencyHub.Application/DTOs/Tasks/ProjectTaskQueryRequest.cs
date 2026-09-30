namespace AgencyHub.Application.DTOs.Tasks;

public class ProjectTaskQueryRequest
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public Guid? ProjectId { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}