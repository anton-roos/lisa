using Lisa.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public LearnerStatus Status { get; set; } = LearnerStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? RegisterClassId { get; set; }
    public RegisterClass? RegisterClass { get; set; }
    public bool IsDisabled { get; set; } = false; //Added missing property
    
    // Previous grade/class tracking for year-end promotion workflow
    public Guid? PreviousSchoolGradeId { get; set; }
    public SchoolGrade? PreviousSchoolGrade { get; set; }
    public Guid? PreviousRegisterClassId { get; set; }
    public RegisterClass? PreviousRegisterClass { get; set; }
    
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

    /// <summary>
    /// Gets the learner's display name in format: "Surname, Name (Grade)"
    /// Safely handles null RegisterClass by falling back to PreviousSchoolGrade or showing "No Grade"
    /// </summary>
    [NotMapped]
    public string DisplayName
    {
        get
        {
            var gradeName = RegisterClass?.SchoolGrade?.SystemGrade?.Name 
                         ?? PreviousSchoolGrade?.SystemGrade?.Name 
                         ?? "No Grade";
            return $"{Surname ?? ""}, {Name ?? ""} {gradeName}".Trim();
        }
    }

    /// <summary>
    /// Gets the learner's full name without grade: "Surname, Name"
    /// </summary>
    [NotMapped]
    public string FullName => $"{Surname ?? ""}, {Name ?? ""}".Trim(' ', ',');

    /// <summary>
    /// Gets the current grade name, safely handling null navigation properties
    /// Falls back to PreviousSchoolGrade if RegisterClass is null
    /// </summary>
    [NotMapped]
    public string? GradeName => RegisterClass?.SchoolGrade?.SystemGrade?.Name 
                             ?? PreviousSchoolGrade?.SystemGrade?.Name;
}

public enum MedicalTransport
{
    None,
    PrivateAmbulance,
    PublicAmbulance,
}
