namespace Lisa.Models.Entities;

public class SchoolManagement : User
{
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

}
