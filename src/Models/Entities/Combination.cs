using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lisa.Models.Entities;

public class Combination : AcademicEntity
{
    [MaxLength(30)]
    public string? Name { get; set; }
    public Guid SchoolGradeId { get; set; }
    public SchoolGrade? SchoolGrade { get; set; }
    public CombinationType Type { get; set; }
    public ICollection<Subject>? Subjects { get; set; }

    /// <summary>
    /// Gets the combination's grade name, safely handling null navigation properties.
    /// </summary>
    [NotMapped]
    public string? GradeName => SchoolGrade?.SystemGrade?.Name;

    /// <summary>
    /// Gets the combination's display name in format: "Name Grade".
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

public enum CombinationType
{
    None,
    SubjectCombination,
    MathCombination
}