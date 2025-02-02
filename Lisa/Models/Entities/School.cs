namespace Lisa.Models.Entities;

public class School
{
    public Guid Id { get; set; }
    public string? ShortName { get; set; }
    public string? LongName { get; set; }
    public string? Color { get; set; }
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string? SmtpEmail { get; set; }
    public string? SmtpPassword { get; set; }
    public Guid SchoolTypeId { get; set; }
    public Guid SchoolCurriculumId { get; set; }
    public SchoolType? SchoolType { get; set; }
    public SchoolCurriculum? Curriculum { get; set; }
    public ICollection<SchoolGrade>? SchoolGrades { get; set; }
    public ICollection<RegisterClass>? RegisterClasses { get; set; }
    public ICollection<Teacher>? Teachers { get; set; }
    public ICollection<Principal>? Principals { get; set; }
    public ICollection<Administrator>? Administrators { get; set; }
    public ICollection<SchoolManagement>? SchoolManagements { get; set; }
    public ICollection<Learner>? Learners { get; set; }
    public ICollection<CareGroup>? CareGroups { get; set; }
    public ICollection<Period>? Periods { get; set; }
}
