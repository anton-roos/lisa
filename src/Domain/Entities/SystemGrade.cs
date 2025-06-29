using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class SystemGrade
{
    public int Id { get; set; }
    public int SequenceNumber { get; set; }
    [MaxLength(16)]
    public string? Name { get; set; }
    public bool MathGrade { get; set; }
    public bool CombinationGrade { get; set; }
    public bool AchievementLevelRating { get; set; }
}
