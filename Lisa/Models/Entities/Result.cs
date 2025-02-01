namespace Lisa.Models.Entities;

public class Result
{
    public Guid Id { get; set; }
    public Guid LearnerId { get; set; }
    public int SubjectId { get; set; }
    public int? Score { get; set; }
    public DateTime ResultDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? AssessmentType { get; set; }
    public string? AssessmentTopic { get; set; }
    public string? CapturedBy { get; set; }
    public bool Absent { get; set; }
    public string? AbsentReason { get; set; }
    public Learner? Learner { get; set; }
    public Subject? Subject { get; set; }
}
