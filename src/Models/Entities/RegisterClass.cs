using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lisa.Models.Entities;

public class RegisterClass : AcademicEntity
{
    public RegisterClass()
    {
        Id = Guid.NewGuid();
    }

    [MaxLength(32)]
    public string? Name { get; set; }
    public Guid SchoolGradeId { get; set; }
    public SchoolGrade? SchoolGrade { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public ICollection<Learner>? Learners { get; set; }
    public ICollection<Subject>? CompulsorySubjects { get; set; }

    /// <summary>
    /// Gets the register class's grade name, safely handling null navigation properties.
    /// </summary>
    [NotMapped]
    public string? GradeName => SchoolGrade?.SystemGrade?.Name;

    /// <summary>
    /// Gets the register class's display name in format: "Name Grade".
    /// Falls back to "No Grade" if the grade is not loaded.
    /// </summary>
    [NotMapped]
    public string DisplayName
    {
        get
        {
            var gradeName = GradeName ?? "No Grade";
            return $"{Name ?? ""} {gradeName}".Trim();
        }
    }
}
