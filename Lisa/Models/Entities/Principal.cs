namespace Lisa.Models.Entities;

public class Principal : User
{
    string? FirstName { get; set; }
    string? LastName { get; set; }
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
}
