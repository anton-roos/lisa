using System.ComponentModel.DataAnnotations;

namespace Lisa.Domain.Entities;

public class RegisterClass
{
    public Guid Id { get; set; }
    [MaxLength(32)]
    public string? Name { get; set; }
    public Guid SchoolGradeId { get; set; }
    public SchoolGrade? SchoolGrade { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public ICollection<Learner>? Learners { get; set; }
    public ICollection<Subject>? CompulsorySubjects { get; set; }
}
