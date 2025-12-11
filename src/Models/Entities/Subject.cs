using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class Subject
{
    public int Id { get; set; }
    [MaxLength(32)]
    public string? Name { get; set; }
    [MaxLength(64)]
    public string? Description { get; set; }
    [MaxLength(16)]
    public string? Code { get; set; }
    public int Order { get; set; }
    public List<int>? GradesApplicable { get; set; }
    public SubjectType SubjectType { get; set; }
}

public enum SubjectType
{
    Compulsory,
    Combination,
    MathCombination,
    AdditionalSubject
}