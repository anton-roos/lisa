using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

/// <summary>
/// Join entity linking learners to ADI (Academic Development Intervention) classes.
/// Tracks whether a learner was originally assigned or added as an additional participant.
/// </summary>
public class AdiLearner : Entity
{
    [Required]
    public Guid AcademicDevelopmentClassId { get; set; }
    public AcademicDevelopmentClass? AcademicDevelopmentClass { get; set; }

    [Required]
    public Guid LearnerId { get; set; }
    public Learner? Learner { get; set; }

    /// <summary>
    /// Indicates if this learner was added during attendance (not on original list).
    /// Additional learners show with the "Additional" pill.
    /// </summary>
    public bool IsAdditional { get; set; }

    /// <summary>
    /// The reason why this learner is in a Break ADI class.
    /// Required when adding learners to Break ADI type.
    /// </summary>
    [MaxLength(500)]
    public string? BreakReason { get; set; }
}
