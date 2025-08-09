using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class CareGroup
{
    public Guid Id { get; set; }
    [MaxLength(30)]
    public string? Name { get; set; }
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
    public ICollection<Learner>? CareGroupMembers { get; set; }
    public ICollection<User>? Users { get; set; }
}
