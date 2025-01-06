using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class SystemAdminViewModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    public string? Email { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
    public string? Password { get; set; }
}
