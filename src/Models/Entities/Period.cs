using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class Period
{
    public Guid Id { get; set; }
    public Guid? SchoolId { get; set; }
    public School? School { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public PeriodStatus Status { get; set; }
    [MaxLength(32)]
    public string? Description { get; set; }
    public Guid SchoolGradeId { get; set; }
    public SchoolGrade? SchoolGrade { get; set; }
    public Guid TeacherId { get; set; }
    public User? Teacher { get; set; }
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
}
