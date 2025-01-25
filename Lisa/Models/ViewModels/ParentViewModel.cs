using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class ParentViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "First Name is required")]
    public string? FirstName { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name is required")]
    public string? LastName { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Primary Email is required")]
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Primary Cell Number is required")]
    public string? PrimaryCellNumber { get; set; }
    public string? SecondaryCellNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Relationship is required")]
    public string? Relationship { get; set; }
}
