using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

/// <summary>
/// Join table for Break ADIs that can have multiple subjects.
/// </summary>
public class AdiSubject
{
    [Required]
    public Guid AcademicDevelopmentClassId { get; set; }
    public AcademicDevelopmentClass? AcademicDevelopmentClass { get; set; }

    [Required]
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
}
