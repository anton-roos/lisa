using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class EmailTemplate
{
    public Guid Id { get; set; }
    [MaxLength(30)]
    public string? Name { get; set; }
    [MaxLength(30)]
    public string? Subject { get; set; }
    [MaxLength(8192)]
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
