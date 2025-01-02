namespace Lisa.Models.Entities;

public class LearnerParent
{
    public Guid Id { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CellNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? Relationship { get; set; }
    public Guid LearnerId { get; set; }
    public Learner? Learner { get; set; }
}
