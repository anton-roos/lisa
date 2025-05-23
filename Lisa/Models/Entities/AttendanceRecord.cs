using Lisa.Enums;

namespace Lisa.Models.Entities;

public class AttendanceRecord : Entity
{
    public Guid AttendanceId { get; set; }
    public Attendance Attendance { get; set; } = null!;
    public Guid LearnerId { get; set; }
    public Learner? Learner { get; set; }
    public DateTimeOffset? Start { get; set; }
    public DateTime? End { get; set; }
    public string? Notes { get; set; }
    public AttendanceType AttendanceType { get; set; }
}