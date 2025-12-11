using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class AcademicYear
{
    public Guid Id { get; set; }
    
    [Range(1900, 9999)]
    public int Year { get; set; }
    
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
    
    public bool IsCurrent { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
