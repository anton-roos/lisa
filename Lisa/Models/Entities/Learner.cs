using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class Learner
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? IdNumber { get; set; }
    public string? Email { get; set; }
    public string? CellNumber { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? RegisterClassId { get; set; }
    public RegisterClass? RegisterClass { get; set; }
    public ICollection<Parent>? Parents { get; set; }
    public ICollection<Result>? Results { get; set; }
    public Guid? CareGroupId { get; set; }
    public CareGroup? CareGroup { get; set; }
    public Guid? CombinationId { get; set; }
    public Combination? Combination { get; set; }
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
    public ICollection<LearnerSubject>? LearnerSubjects { get; set; }
}
