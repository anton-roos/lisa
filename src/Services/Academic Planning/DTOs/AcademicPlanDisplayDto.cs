using System;

namespace Lisa.Services.AcademicPlanning.DTOs
{
    public class AcademicPlanDisplayDto
    {
        public Guid Id { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Term { get; set; }
        public Lisa.Enums.AcademicPlanStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsCatchUpPlan { get; set; }
    }
}
