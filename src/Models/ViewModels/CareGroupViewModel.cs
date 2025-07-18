using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class CareGroupViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is Required")]
    public string? Name { get; set; }
}