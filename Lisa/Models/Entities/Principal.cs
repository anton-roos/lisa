namespace Lisa.Models.Entities;

public class Principal : User
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
}
