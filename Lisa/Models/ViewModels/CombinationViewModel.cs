using Lisa.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class CombinationViewModel
{
    public Guid? Id { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string? Name { get; set; }
    public Guid GradeId { get; set; }
    public List<int> SubjectIds { get; set; } = [];
    [Required(ErrorMessage = "Combination Type is required")]
    public CombinationType CombinationType { get; set; }
}
