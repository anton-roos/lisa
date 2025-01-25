using System.ComponentModel.DataAnnotations;
using Lisa.Models.Entities;

namespace Lisa.Models.ViewModels;

public class CombinationViewModel
{
    public Guid? Id { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string? Name { get; set; }
    public Guid GradeId { get; set; }
    public List<Guid> SubjectIds { get; set; } = [];
    [Required(ErrorMessage = "Combination Type is required")]
    public CombinationType CombinationType { get; set; }
}
