using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        [MaxLength(64)]
        public string ActivityType { get; set; } = string.Empty;
        [MaxLength(512)]
        public string Description { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}