namespace Lisa.Models.Entities;

public class LearnerSubject : AcademicEntity
{
    public Guid LearnerId { get; set; }
    public Learner Learner { get; set; } = null!;
    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    public LearnerSubjectType LearnerSubjectType { get; set; }
    public Guid? CombinationId { get; set; }
    public Combination? Combination { get; set; }
}

public enum LearnerSubjectType
{
    Compulsory,
    Combination,
    MathCombination,
    Additional
}
