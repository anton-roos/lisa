using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class AdministratorViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    public string? Email { get; set; }
}