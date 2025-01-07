namespace Lisa.Models.Entities;

public class Teacher : User
{
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
    public ICollection<Subject>? Subjects { get; set; }
    public ICollection<RegisterClass>? RegisterClasses { get; set; }
    public ICollection<Period>? Periods { get; set; }
}
