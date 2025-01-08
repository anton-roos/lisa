namespace Lisa.Models.Entities;
public class Grade
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int SequenceNumber { get; set; }
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
    public ICollection<RegisterClass>? RegisterClasses { get; set; }
    public ICollection<Combination>? Combinations { get; set; }
}
