namespace AgencyHub.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ClientId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = "Planning";

    public decimal Budget { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties

    public Client Client { get; set; } = null!;
}