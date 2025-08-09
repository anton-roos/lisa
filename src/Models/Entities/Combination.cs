using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class Combination
{
    public Guid Id { get; set; }
    [MaxLength(30)]
    public string? Name { get; set; }
    public Guid SchoolGradeId { get; set; }
    public SchoolGrade? SchoolGrade { get; set; }
    public CombinationType CombinationType { get; set; }
    public ICollection<Subject>? Subjects { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
