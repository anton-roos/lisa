namespace Lisa.Models.Entities;

public class ResultSet
{
    public Guid Id { get; set; }
    public DateTime? AssessmentDate { get; set; }
    public string? AssessmentType { get; set; }
    public string? AssessmentTopic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CapturedById { get; set; }
    public User? CapturedByUser { get; set; }
    public ICollection<Result>? Results { get; set; }
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
}
