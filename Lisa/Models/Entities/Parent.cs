namespace Lisa.Models.Entities;

public class Parent
{
    public Guid Id { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? Surname { get; set; }
    public string? Name { get; set; }
    public string? PrimaryCellNumber { get; set; }
    public string? SecondaryCellNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? Relationship { get; set; }
    public Guid LearnerId { get; set; }
    public Learner? Learner { get; set; }
    public ICollection<EmailRecipient>? EmailReceipts { get; set; }
}
