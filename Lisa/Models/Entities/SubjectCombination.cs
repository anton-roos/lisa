namespace Lisa.Models.Entities;

public class SubjectCombination
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid GradeId { get; set; }
    public Grade? Grade { get; set; }
    public ICollection<SubjectCombinationSubject>? SubjectCombinationSubjects { get; set; }
}
