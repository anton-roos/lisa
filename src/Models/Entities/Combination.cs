using System.ComponentModel.DataAnnotations;
using Lisa.Models;

namespace Lisa.Models.Entities;

public class Combination : AcademicEntity
{
    [MaxLength(30)]
    public string? Name { get; set; }
    public Guid SchoolGradeId { get; set; }
    public SchoolGrade? SchoolGrade { get; set; }
    public CombinationType Type { get; set; }
    public ICollection<Subject>? Subjects { get; set; }
}

public enum CombinationType
{
    None,
    SubjectCombination,
    MathCombination
}