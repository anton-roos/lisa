using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class ResultViewModel
{
    [Required(ErrorMessage = "Please enter a mark.")]
    [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
    public int? Score { get; set; }
    public bool Absent { get; set; } = false;
    public string? AbsentReason { get; set; }

    [Required(ErrorMessage = "Please enter an Assessment Type.")]
    public string? AssessmentType { get; set; }

    [Required(ErrorMessage = "Please enter a Assessment Topic.")]
    public string? AssessmentTopic { get; set; }

    [Required(ErrorMessage = "Assessment date is required.")]
    public DateTime? AsessmentDate { get; set; }
}
