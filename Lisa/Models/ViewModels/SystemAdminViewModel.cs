using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class SystemAdminViewModel
{
    public string? Surname { get; set; }
    public string? Name { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    public string? Email { get; set; }
    public string? Password { get; set; }
}
