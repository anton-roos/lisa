namespace Lisa.Models.Entities;

public class Result
{
    public Guid Id { get; set; }
    public Guid LearnerId { get; set; }
    public Guid SubjectId { get; set; }
    public decimal Score { get; set; }
    public DateTime ResultDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CapturedBy { get; set; }
    public bool Absent { get; set; }
    public string? AbsentReason { get; set; }
    public Learner? Learner { get; set; }
    public Subject? Subject { get; set; }
}
