using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class LearnerParentViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Surname is required")]
    public string? Surname { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string? Name { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Primary Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? PrimaryEmail { get; set; }
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? SecondaryEmail { get; set; }
    public string? CellNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? Relationship { get; set; }
}
