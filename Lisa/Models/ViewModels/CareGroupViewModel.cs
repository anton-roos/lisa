using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class CareGroupViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    public string? Email { get; set; }
}