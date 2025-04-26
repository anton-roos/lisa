using System;

namespace Lisa.Models.Entities;

public class Attendance : Entity
{
    public Guid LearnerId { get; set; }
    public Learner? Learner { get; set; }

    public Guid SchoolId { get; set; }
    public School? School { get; set; }

    public Guid RegisterClassId { get; set; }
    public RegisterClass? RegisterClass { get; set; }

    public DateTime Date { get; set; }

    public bool IsPresent { get; set; }

    public DateTime? SignInTime { get; set; }

    public DateTime? SignOutTime { get; set; }

    public bool IsEarlyLeave { get; set; }

    public string? Notes { get; set; }

    // Who recorded the attendance
    public Guid? RecordedByUserId { get; set; }
    public User? RecordedByUser { get; set; }

    // Add this property to your existing Attendance entity
    public Guid? AttendanceSessionId { get; set; }
    public AttendanceSession? AttendanceSession { get; set; }
}