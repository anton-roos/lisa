namespace Lisa.Models.Entities;

public class AttendanceSession : Entity
{
    public Guid SchoolId { get; set; }
    public School School { get; set; } = null!;

    public DateTime Date { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public Guid? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}