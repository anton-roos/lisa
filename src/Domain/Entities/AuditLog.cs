namespace Lisa.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public Guid? UserId { get; set; }
    public DateTime Timestamp { get; set; }
}
