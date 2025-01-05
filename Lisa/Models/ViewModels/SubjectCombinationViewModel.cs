using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class SubjectCombinationViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string? Name { get; set; }
    // Grade must be selected before a subject combination can be created
    public Guid GradeId { get; set; }
   public List<Guid> SubjectIds { get; set; } = new();
}