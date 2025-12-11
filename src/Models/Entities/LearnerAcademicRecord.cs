using System.ComponentModel.DataAnnotations;
using Lisa.Enums;

namespace Lisa.Models.Entities;

public class LearnerAcademicRecord : AcademicEntity
{
    public Guid LearnerId { get; set; }
    public Learner? Learner { get; set; }
    public Guid SchoolGradeId { get; set; }
    public SchoolGrade? SchoolGrade { get; set; }
    public Guid? RegisterClassId { get; set; }
    public RegisterClass? RegisterClass { get; set; }
    public Guid? CombinationId { get; set; }
    public Combination? Combination { get; set; }
    
    // JSON string of subject names/IDs
    public string? SubjectSnapshot { get; set; }
    [MaxLength(512)]
    public string? Comment { get; set; }
    public PromotionStatus Outcome { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
