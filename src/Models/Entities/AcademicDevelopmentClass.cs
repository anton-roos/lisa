using System.ComponentModel.DataAnnotations;
using Lisa.Enums;

namespace Lisa.Models.Entities;

public class AcademicDevelopmentClass : AcademicEntity
{
    [Required]
    public DateTime DateTime { get; set; }

    /// <summary>
    /// The type of ADI: Support (grade-focused) or Break (multi-grade).
    /// </summary>
    public AdiType AdiType { get; set; } = AdiType.Support;

    /// <summary>
    /// The grade for Support ADIs. Null for Break ADIs which are multi-grade.
    /// </summary>
    public Guid? SchoolGradeId { get; set; }
    public SchoolGrade? SchoolGrade { get; set; }

    [Required]
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public Guid? TeacherId { get; set; }
    public User? Teacher { get; set; }

    [Required]
    public Guid SchoolId { get; set; }
    public School? School { get; set; }

    /// <summary>
    /// Indicates whether attendance tracking is currently open for this ADI class.
    /// </summary>
    public bool IsAttendanceOpen { get; set; }

    /// <summary>
    /// The time when attendance tracking was started (UTC).
    /// </summary>
    public DateTime? AttendanceStartedAt { get; set; }

    /// <summary>
    /// The time when attendance tracking was stopped (UTC).
    /// Learners arriving after this time are marked as late.
    /// </summary>
    public DateTime? AttendanceStoppedAt { get; set; }

    /// <summary>
    /// Collection of learners assigned to this ADI class.
    /// </summary>
    public ICollection<AdiLearner> AdiLearners { get; set; } = [];
}
