using Lisa.Enums;
using Lisa.Models;

namespace Lisa.Models.Entities;

public class Attendance : AcademicEntity
{
    public Guid SchoolId { get; set; }
    public School School { get; set; } = null!;
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public AttendanceType Type { get; set; }
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}