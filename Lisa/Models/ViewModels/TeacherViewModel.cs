using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class TeacherViewModel
{
    public Guid Id { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Surname is required")]
    public string Surname { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    public Guid? SchoolId { get; set; }
}
