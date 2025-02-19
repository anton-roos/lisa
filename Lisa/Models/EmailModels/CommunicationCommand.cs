using System.ComponentModel.DataAnnotations;
using Lisa.Enums;

namespace Lisa.Models.EmailModels;

public class CommunicationCommand
{
    public Guid SchoolId { get; set; }
    [Required]
    public CommunicationTarget Target { get; set; }
    [Required]
    public Audience Audience { get; set; }
    public Guid? GradeId { get; set; }
    [Required]
    public int SubjectId { get; set; }
    public string? SubjectLine { get; set; }
    public string? SenderName { get; set; }
    [EmailAddress]
    public string? SenderEmail { get; set; }
    public Guid? LearnerId { get; set; }
    public ProgressFeedback? ProgressReport { get; set; }
    public string TemplateModelType { get; set; } = string.Empty;
    public Template EmailTemplate { get; set; }
}
