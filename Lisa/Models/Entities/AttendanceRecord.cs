using Lisa.Enums;

namespace Lisa.Models.Entities;

public class AttendanceRecord : Entity
{
    public Guid AttendanceId { get; set; }
    public Attendance Attendance { get; set; } = null!;
    public Guid LearnerId { get; set; }
    public Learner? Learner { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public string? Notes { get; set; }
    public AttendanceType AttendanceType { get; set; }
    public bool CellPhoneCollected { get; set; } = false;
}