using Lisa.Enums;
using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class Learner
{
    public Learner()
    {
        Id = Guid.NewGuid();
        SchoolId = Guid.NewGuid();
    }

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
    [MaxLength(64)]
    public string? MedicalAidName { get; set; }
    [MaxLength(64)]
    public string? MedicalAidNumber { get; set; }
    [MaxLength(64)]
    public string? MedicalAidPlan { get; set; }
    [MaxLength(256)]
    public string? Allergies { get; set; }
    [MaxLength(512)]
    public string? MedicalAilments { get; set; }
    [MaxLength(512)]
    public string? MedicalInstructions { get; set; }
    [MaxLength(512)]
    public string? DietaryRequirements { get; set; }
    public MedicalTransport MedicalTransport { get; set; }
    public Gender Gender { get; set; }
    
    // Soft delete properties
    public bool IsDisabled { get; set; } = false;
    public DateTime? DisabledAt { get; set; }
    [MaxLength(500)]
    public string? DisabledReason { get; set; }
}

public enum MedicalTransport
{
    None,
    PrivateAmbulance,
    PublicAmbulance,
}
