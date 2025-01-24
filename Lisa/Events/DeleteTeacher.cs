using Lisa.Models.Entities;

namespace Lisa.Events;

public class DeleteTeacherEvent
{
    public string? TeacherId { get; set; }
}
