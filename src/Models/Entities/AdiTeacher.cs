using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

/// <summary>
/// Join table for Break ADIs that can have multiple teachers.
/// </summary>
public class AdiTeacher
{
    [Required]
    public Guid AcademicDevelopmentClassId { get; set; }
    public AcademicDevelopmentClass? AcademicDevelopmentClass { get; set; }

    [Required]
    public Guid TeacherId { get; set; }
    public User? Teacher { get; set; }
}
