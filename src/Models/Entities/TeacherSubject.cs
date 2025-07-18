namespace Lisa.Models.Entities;

public class TeacherSubject
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    public int Grade { get; set; }
}
