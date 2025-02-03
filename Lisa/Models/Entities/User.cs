using System.ComponentModel.DataAnnotations.Schema;
using Lisa.Data;
using Microsoft.AspNetCore.Identity;

namespace Lisa.Models.Entities;

public class User : IdentityUser<Guid>
{
    public string? Surname { get; set; }
    public string? Abbreviation { get; set; }
    public string? Name { get; set; }
    public Guid? SchoolId { get; set; }
    public School? School { get; set; }
    public string? UserType { get; set; }
    public ICollection<EmailRecipient>? EmailReceipts { get; set; }

    [NotMapped]
    public List<string> Roles { get; set; } = [];
}
