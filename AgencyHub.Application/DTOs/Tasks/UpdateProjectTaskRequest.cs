namespace AgencyHub.Application.DTOs.Tasks;

public class UpdateProjectTaskRequest
{
    public Guid ProjectId { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = "Todo";

    public string Priority { get; set; } = "Medium";

    public DateTime? DueDate { get; set; }
}