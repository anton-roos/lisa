namespace Lisa.Models.Entities;

public class Combination
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid GradeId { get; set; }
    public Grade? Grade { get; set; }
    public CombinationType CombinationType { get; set; }
    public ICollection<Subject>? Subjects { get; set; }
}
