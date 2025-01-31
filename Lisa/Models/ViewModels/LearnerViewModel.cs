using System.ComponentModel.DataAnnotations;
using Lisa.Models.Entities;

namespace Lisa.Models.ViewModels;

public class LearnerViewModel
{
    public Guid? Id { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Learner Code is required")]
    public string? Code { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Surname is required")]
    public string? Surname { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string? Name { get; set; }
    public string? IdNumber { get; set; }
    public string? Email { get; set; }
    public string? CellNumber { get; set; }
    public bool Active { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Student Needs to be assigned to a Register Class.")]
    public Guid? RegisterClassId { get; set; }
    public Guid SchoolId { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Student needs to be assigned to a Care Group.")]
    public Guid? CareGroupId { get; set; }
    public List<int> SubjectIds { get; set; } = [];
    public ICollection<Parent>? Parents { get; set; }
}
