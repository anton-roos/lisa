namespace Lisa.Models.Entities;

public class Result
{
    public Guid Id { get; set; }
    public Guid LearnerId { get; set; }
    public Learner? Learner { get; set; }
    public Guid ResultSetId { get; set; }
    public ResultSet? ResultSet { get; set; }
    public int? Score { get; set; }
    public bool Absent { get; set; }
    public string? AbsentReason { get; set; }
    public DateTime UpdatedAt { get; set; }
}
