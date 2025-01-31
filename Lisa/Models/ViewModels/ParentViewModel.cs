using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class ParentViewModel
{
    public Guid? Id { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Surname is required")]
    public string? Surname { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string? Name { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Primary Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? PrimaryEmail { get; set; }
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? SecondaryEmail { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Primary Cell Number is required")]
    public string? PrimaryCellNumber { get; set; }
    public string? SecondaryCellNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Relationship is required")]
    public string? Relationship { get; set; }
}
