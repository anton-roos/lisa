using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class AcademicDevelopmentClass : AcademicEntity
{
    [Required]
    public DateTime DateTime { get; set; }

    [Required]
    public Guid SchoolGradeId { get; set; }
    public SchoolGrade? SchoolGrade { get; set; }

    [Required]
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public Guid? TeacherId { get; set; }
    public User? Teacher { get; set; }

    [Required]
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
}
