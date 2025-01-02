namespace Lisa.Models.Entities;

public class SubjectCombinationSubject
{
    public Guid SubjectCombinationId { get; set; }
    public Guid SubjectId { get; set; }
    public SubjectCombination? SubjectCombination { get; set; }
    public Subject? Subject { get; set; }
}
