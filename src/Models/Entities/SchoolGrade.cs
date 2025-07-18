namespace Lisa.Models.Entities;
public class SchoolGrade
{
    public Guid Id { get; set; }
    public int SystemGradeId { get; set; }
    public SystemGrade SystemGrade { get; set; } = null!;
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public ICollection<RegisterClass>? RegisterClasses { get; set; }
    public ICollection<Combination>? Combinations { get; set; }
}
