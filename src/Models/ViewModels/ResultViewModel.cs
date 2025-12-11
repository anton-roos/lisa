using Lisa.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Lisa.Models.ViewModels;

public class ResultViewModel
{
    [Required(ErrorMessage = "Please enter a mark.")]
    [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
    public int? Score { get; set; }

    public bool Absent { get; set; }
    public string? AbsentReason { get; set; }

    [Required(ErrorMessage = "Please enter an Assessment Type.")]
    public string? AssessmentType { get; set; }

    [Required(ErrorMessage = "Please enter an Assessment Topic.")]
    public string? AssessmentTopic { get; set; }

    [Required(ErrorMessage = "Assessment date is required.")]
    public DateTime? AssessmentDate { get; set; }
}

public class ResultsCaptureViewModel
{
    public List<SchoolGrade> SchoolGrades { get; set; } = [];
    public List<Subject> FilteredSubjects { get; set; } = [];
    public List<LearnerResultViewModel> LearnerResults { get; set; } = [];
    public SchoolGrade? SchoolGrade { get; set; } = new();

    public ClaimsPrincipal? User { get; set; }
    public bool Loading { get; set; } = true;
    public User? Teacher { get; set; }

    // Query parameters
    public string GradeId { get; set; } = string.Empty;
    public string SubjectId { get; set; } = string.Empty;

    // Assessment fields
    [Required(ErrorMessage = "Assessment topic is required.")]
    public string? AssessmentTopic { get; set; }
    public AssessmentType? AssessmentType { get; set; }
    public DateTime? AssessmentDate { get; set; }
    public ResultSetStatus Status { get; set; }
}

public class LearnerResultViewModel
{
    public Guid LearnerId { get; set; }
    public string Surname { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? GradeName { get; set; }
    public ResultViewModel ResultViewModel { get; set; } = new();
    
    public string DisplayName => string.IsNullOrEmpty(GradeName) 
        ? $"{Surname}, {Name}".Trim(' ', ',') 
        : $"{Surname}, {Name} ({GradeName})".Trim();
}
