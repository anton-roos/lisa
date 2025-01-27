using Lisa.Interfaces;

namespace Lisa.Models.Entities;

public class Period : IValidatable
{
    public Guid Id { get; set; }
    public Guid? SchoolId { get; set; }
    public School? School { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public PeriodStatus Status { get; set; }
    public string? Description { get; set; }
    public Guid GradeId { get; set; }
    public Grade? Grade { get; set; }
    public Guid TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public void Validate()
    {
        if (StartTime >= EndTime)
        {
            throw new InvalidOperationException("Period start time must be before end time.");
        }
    }
}
