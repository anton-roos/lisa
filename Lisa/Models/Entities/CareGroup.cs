namespace Lisa.Models.Entities;

public class CareGroup
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
    public ICollection<Learner>? CareGroupMembers { get; set; }
}