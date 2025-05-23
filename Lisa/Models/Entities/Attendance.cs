namespace Lisa.Models.Entities;

public class Attendance : Entity
{
    public Guid SchoolId { get; set; }
    public School School { get; set; } = null!;
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset? End { get; set; }
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}