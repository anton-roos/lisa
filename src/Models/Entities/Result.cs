using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class Result : Entity
{
    public Guid LearnerId { get; set; }
    public Learner? Learner { get; set; }
    public Guid ResultSetId { get; set; }
    public ResultSet? ResultSet { get; set; }
    public int? Score { get; set; }
    public bool Absent { get; set; }
    [MaxLength(64)]
    public string? AbsentReason { get; set; }
}
