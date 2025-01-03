using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class SubjectViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Subject Code is required")]
    public string? Code { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Subject Name is required")]
    public string? Name { get; set; }
}