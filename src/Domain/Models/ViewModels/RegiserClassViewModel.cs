using Lisa.Models.Entities;

namespace Lisa.Models.ViewModels;

public class RegisterClassViewModel
{
    public string? Name { get; set; }
    public Guid GradeId { get; set; }
    public SchoolGrade? Grade { get; set; }
    public Guid TeacherId { get; set; }
    public List<int> SubjectIds { get; set; } = [];
}
