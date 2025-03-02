using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class Learner
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    [MaxLength(30)]
    public string? Surname { get; set; }
    [MaxLength(30)]
    public string? Name { get; set; }
    [MaxLength(20)]
    public string? IdNumber { get; set; }
    [MaxLength(256)]
    public string? Email { get; set; }
    [MaxLength(30)]
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
    public ICollection<EmailRecipient>? EmailReceipts { get; set; }
    public string? MedicalAidName { get; set; }
    public string? MedicalAidNumber { get; set; }
    public string? MedicalAidPlan { get; set; }
    public string? Allergies { get; set; }
    public string? MedicalAilments { get; set; }
    public string? MedicalInstructions { get; set; }
    public string? DietaryRequirements { get; set; }
    public MedicalTransport MedicalTransport { get; set; }
}

public enum MedicalTransport
{
    None,
    PrivateAmbulance,
    PublicAmbulance,
}
