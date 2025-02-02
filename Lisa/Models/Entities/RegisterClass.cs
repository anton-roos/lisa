namespace Lisa.Models.Entities;

public class RegisterClass
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid GradeId { get; set; }
    public SchoolGrade? SchoolGrade { get; set; }
    public Guid TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public ICollection<Learner>? Learners { get; set; }
    public ICollection<Subject>? CompulsorySubjects { get; set; }
    public bool MathSelectionEnabled { get; set; }
}
