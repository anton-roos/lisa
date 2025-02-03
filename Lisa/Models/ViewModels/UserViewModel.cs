using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class UserViewModel
{
    public Guid? Id { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Surname is required")]
    public string Surname { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Abbreviation is required")]
    public string Abbreviation { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
    public string? Password { get; set; }
    public Guid? SchoolId { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> SelectedRoles { get; set; } = new();
}
