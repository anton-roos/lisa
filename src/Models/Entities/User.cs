using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lisa.Models.Entities;

public class User : IdentityUser<Guid>
{
    public User()
    {
        Id = Guid.NewGuid();
    }

    public string? Surname { get; set; }
    public string? Abbreviation { get; set; }
    [MaxLength(64)]
    public string? Name { get; set; }
    public Guid? SchoolId { get; set; }
    public School? School { get; set; }
    [MaxLength(256)]
    public string? UserType { get; set; }
    public ICollection<EmailRecipient>? EmailReceipts { get; set; }
    [NotMapped]
    public List<string> Roles { get; set; } = [];
    public ICollection<CareGroup>? CareGroups { get; set; }
    public ICollection<RegisterClass>? RegisterClasses { get; set; }
    public ICollection<Period>? Periods { get; set; }
    public ICollection<TeacherSubject>? Subjects { get; set; }
}
