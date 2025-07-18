using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class EventLog
{
    public int Id { get; set; }
    [MaxLength(32)]
    public string? EventType { get; set; }
    [MaxLength(8192)]
    public string? EventData { get; set; }
    public DateTime CreatedAt { get; set; }
}
