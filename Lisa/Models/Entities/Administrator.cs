namespace Lisa.Models.Entities;

public class Administrator : User
{
    public Guid SchoolId { get; set; }
    public School? School { get; set; }

}
