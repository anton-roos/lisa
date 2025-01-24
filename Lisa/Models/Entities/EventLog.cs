namespace Lisa.Models.Entities;

public class EventLog
{
    public int Id { get; set; }
    public string? EventType { get; set; }
    public string? EventData { get; set; }
    public DateTime CreatedAt { get; set; }
}
