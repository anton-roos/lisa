using System.ComponentModel.DataAnnotations;
using Lisa.Enums;

namespace Lisa.Models.EmailModels;

public class CommunicationRequest
{
    public CommunicationTarget Target { get; set; }
    public Guid SchoolId { get; set; }
    public Guid? GradeId { get; set; }
    public int SubjectId { get; set; }
    [Required]
    public Audience Audience { get; set; }
    [Required]
    public Guid TemplateId { get; set; }
    public string? SubjectLine { get; set; }
    public string? SenderName { get; set; }
    [EmailAddress]
    public string? SenderEmail { get; set; }
    public string? ContentHtml { get; set; }
    public string? ContentText { get; set; }
    public string? Description { get; set; }
    public Guid? LearnerId { get; set; }
}
