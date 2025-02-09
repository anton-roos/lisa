using System.ComponentModel.DataAnnotations.Schema;
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
    public ICollection<CareGroup>? CareGroups { get; set; }
    public ICollection<RegisterClass>? RegisterClasses { get; set; }
    public ICollection<Period>? Periods { get; set; }
    public ICollection<TeacherSubject>? Subjects { get; set; }
}
