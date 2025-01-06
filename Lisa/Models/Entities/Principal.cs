namespace Lisa.Models.Entities;

public class Principal : User
{
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
}
