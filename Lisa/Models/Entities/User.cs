
using Microsoft.AspNetCore.Identity;

namespace Lisa.Models.Entities;

public class User : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
