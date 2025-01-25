namespace Lisa.Models.Entities;

public class LearnerSubject
{
    public Guid LearnerId { get; set; }
    public Learner Learner { get; set; } = null!;
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
}
