using Lisa.Enums;
using Lisa.Models.Entities;
using System.ComponentModel.DataAnnotations;

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

    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    public string? CellNumber { get; set; }
    public bool Active { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Learner needs to be assigned to a Register Class.")]
    public Guid? RegisterClassId { get; set; }

    public Guid SchoolId { get; } = Guid.NewGuid();

    [Required(AllowEmptyStrings = false, ErrorMessage = "Learner needs to be assigned to a Care Group.")]
    public Guid? CareGroupId { get; set; }

    public List<int> SubjectIds { get; set; } = [];

    public Dictionary<Guid, int> CombinationSelections { get; set; } = new();

    public int MathSelection { get; set; }

    public List<int> ExtraSubjectIds { get; set; } = [];

    public string? MedicalAidName { get; set; }

    public string? MedicalAidNumber { get; set; }

    public string? MedicalAidPlan { get; set; }

    public string? Allergies { get; set; }

    public string? MedicalAilments { get; set; }

    public string? MedicalInstructions { get; set; }

    public string? DietaryRequirements { get; set; }

    public MedicalTransport MedicalTransport { get; set; }
    public Gender Gender { get; set; }
}
