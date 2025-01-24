using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class TeacherViewModel
{
    public Guid Id { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    public Guid? SchoolId { get; set; }
}
