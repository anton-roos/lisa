using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class SubjectCombinationViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string? Name { get; set; }
    public Guid GradeId { get; set; }
    public List<Guid> SubjectIds { get; set; } = [];
}