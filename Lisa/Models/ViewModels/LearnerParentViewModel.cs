using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class LearnerParentViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Primary Email is required")]
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Surname is required")]
    public string? Surname { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string? Name { get; set; }
    public string? CellNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? Relationship { get; set; }
}
