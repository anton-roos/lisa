using Lisa.Models.Entities;

namespace Lisa.Events;

public class AddTeacherEvent
{
    public Teacher? Teacher { get; set; }
}
