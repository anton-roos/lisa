using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class SchoolCurriculum
{
    public Guid Id { get; set; }
    [MaxLength(16)]
    public string? Name { get; set; }
    [MaxLength(256)]
    public string? Description { get; set; }
}
