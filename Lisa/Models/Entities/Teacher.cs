namespace Lisa.Models.Entities;

public class Teacher : User
{
    public ICollection<Subject>? Subjects { get; set; }
    public  string? Abbreviation { get; set; }
    public ICollection<RegisterClass>? RegisterClasses { get; set; }
    public ICollection<Period>? Periods { get; set; }
}
