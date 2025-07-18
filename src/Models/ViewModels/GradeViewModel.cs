using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class GradeViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string? Name { get; set; }
    public int SequenceNumber { get; set; }
}
