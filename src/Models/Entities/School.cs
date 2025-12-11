using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class School
{
    public School()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; set; }
    [MaxLength(8)]
    public string? ShortName { get; set; }
    [MaxLength(64)]
    public string? LongName { get; set; }
    [MaxLength(8)]
    public string? Color { get; set; }
    [MaxLength(32)]
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    [MaxLength(256)]
    public string? SmtpEmail { get; set; }
    [MaxLength(256)]
    public string? SmtpPassword { get; set; }
    [MaxLength(256)]
    public string? SmtpUsername { get; set; }
    [MaxLength(256)]
    public string? FromEmail { get; set; }
    public Guid SchoolTypeId { get; set; }
    public Guid SchoolCurriculumId { get; set; }
    public SchoolType? SchoolType { get; set; }
    public SchoolCurriculum? Curriculum { get; set; }
    public ICollection<SchoolGrade>? SchoolGrades { get; set; }
    public ICollection<RegisterClass>? RegisterClasses { get; set; }
    public ICollection<User>? Staff { get; set; }
    public ICollection<Learner>? Learners { get; set; }
    public ICollection<CareGroup>? CareGroups { get; set; }
    public ICollection<Period>? Periods { get; set; }
    public bool IsYearEndMode { get; set; }
}
