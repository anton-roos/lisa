using Lisa.Enums;

namespace Lisa.Models.EmailModels;

public class CommunicationCommand
{
    public Guid SchoolId { get; set; }
    public RecipientGroup RecipientGroup { get; set; }
    public RecipientType RecipientType { get; set; }
    public RecipientTemplate RecipientTemplate { get; set; }
    public Guid? GradeId { get; set; }
    public int SubjectId { get; set; }
    public Guid? LearnerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? CampaignId { get; set; }
}
