namespace Lisa.Models.Entities;

public class Subject
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Code { get; set; }
    public ICollection<Combination>? Combinations { get; set; }
    public ICollection<LearnerSubject>? LearnerSubjects { get; set; }
}
