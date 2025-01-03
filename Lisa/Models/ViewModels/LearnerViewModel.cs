using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class LearnerViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Learner Code is required")]
    public string? Code { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool Active { get; set; }
    public Guid? RegisterClassId { get; set; }
    public Guid? SubjectCombinationId { get; set; }
    public Guid SchoolId { get; set; }
}
