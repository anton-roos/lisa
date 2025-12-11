using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class ResultSet : AcademicEntity
{
    public DateTime? AssessmentDate { get; set; }
    [MaxLength(32)]
    public string? AssessmentTypeName { get; set; }
    [MaxLength(128)]
    public string? AssessmentTopic { get; set; }
    public Guid CapturedById { get; set; }
    public User? CapturedByUser { get; set; }
    public Guid? TeacherId { get; set; }
    public User? Teacher { get; set; }
    public ICollection<Result>? Results { get; set; }
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public ResultSetStatus Status { get; set; }
    public Guid? SchoolGradeId { get; set; }
    public SchoolGrade? SchoolGrade { get; set; }
    public int AssessmentTypeId { get; set; }

    // Navigation property
    public AssessmentType? AssessmentType { get; set; }
}

public enum ResultSetStatus
{
    Draft,
    Submitted
}
