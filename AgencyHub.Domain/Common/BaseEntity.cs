namespace AgencyHub.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public string TenantId { get; set; } = string.Empty; // Multi-tenant isolation field
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}