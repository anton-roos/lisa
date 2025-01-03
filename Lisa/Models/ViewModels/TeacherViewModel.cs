using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class TeacherViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    public string? Email { get; set; }
}