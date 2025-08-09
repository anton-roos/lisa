namespace Lisa.Models.Entities;

public class AttendanceSession : Entity
{
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
    public DateTime Date { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    // Collection of attendances associated with this session
    public ICollection<Attendance> Attendances { get; set; } = [];
}
