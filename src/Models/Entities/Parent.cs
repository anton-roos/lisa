using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class Parent
{
    public Parent()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; set; }
    [MaxLength(256)]
    public string? PrimaryEmail { get; set; }
    [MaxLength(256)]
    public string? SecondaryEmail { get; set; }
    [MaxLength(32)]
    public string? Surname { get; set; }
    [MaxLength(32)]
    public string? Name { get; set; }
    [MaxLength(16)]
    public string? PrimaryCellNumber { get; set; }
    [MaxLength(16)]
    public string? SecondaryCellNumber { get; set; }
    [MaxLength(16)]
    public string? WhatsAppNumber { get; set; }
    [MaxLength(32)]
    public string? Relationship { get; set; }
    public Guid LearnerId { get; set; }
    public Learner? Learner { get; set; }
    public ICollection<EmailRecipient>? EmailReceipts { get; set; }
}
