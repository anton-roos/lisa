using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class Parent
{
    public Guid Id { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PrimaryCellNumber { get; set; }
    public string? SecondaryCellNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? Relationship { get; set; }
    public Guid LearnerId { get; set; }
    public Learner? Learner { get; set; }
}
