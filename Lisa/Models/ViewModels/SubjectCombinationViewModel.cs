using System.ComponentModel.DataAnnotations;
using Lisa.Models.Entities;

namespace Lisa.Models.ViewModels;

public class SubjectCombinationViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string? Name { get; set; }
    // Grade must be selected before a subject combination can be created
    public Guid GradeId { get; set; }
    public ICollection<Subject>? SubjectsToAdd { get; set; }
}