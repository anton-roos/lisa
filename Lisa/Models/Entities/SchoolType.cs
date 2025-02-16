using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class SchoolType
{
    public Guid Id { get; set; }
    [MaxLength(32)]
    public string? Name { get; set; }
}