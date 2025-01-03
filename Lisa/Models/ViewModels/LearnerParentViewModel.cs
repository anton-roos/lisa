using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class LearnerParentViewModel
{
    [Required (AllowEmptyStrings = false, ErrorMessage = "Primary Email is required")] 
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CellNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? Relationship { get; set; }
}