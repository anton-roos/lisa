namespace Lisa.Models.Entities;

public class Subject
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Code { get; set; }
    public int Order { get; set; }
    public SubjectType SubjectType { get; set; }
    public ICollection<Combination>? Combinations { get; set; }
    public ICollection<LearnerSubject>? LearnerSubjects { get; set; }
}
