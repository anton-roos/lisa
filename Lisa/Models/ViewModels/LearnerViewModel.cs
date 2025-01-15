using System.ComponentModel.DataAnnotations;
using Lisa.Models.Entities;

namespace Lisa.Models.ViewModels;

public class LearnerViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Learner Code is required")]
    public string? Code { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? IdNumber { get; set; }
    public string? Email { get; set; }
    public string? CellNumber { get; set; }
    public bool Active { get; set; }
    public Guid? RegisterClassId { get; set; }
    public Guid SchoolId { get; set; }
    public Guid? CareGroupId { get; set; }
    public List<Guid> SubjectIds { get; set; } = [];
    public ICollection<Parent>? Parents { get; set; }
}
