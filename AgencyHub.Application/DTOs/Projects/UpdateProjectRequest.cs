namespace AgencyHub.Application.DTOs.Projects;

public class UpdateProjectRequest
{
    public Guid ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Planning";
    public decimal Budget { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}