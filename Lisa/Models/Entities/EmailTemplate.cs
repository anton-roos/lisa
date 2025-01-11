namespace Lisa.Models.Entities;

public class EmailTemplate
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Subject { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}