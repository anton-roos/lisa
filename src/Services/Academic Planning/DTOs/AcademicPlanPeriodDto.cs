using System;

namespace Lisa.Services.AcademicPlanning.DTOs
{
    public class AcademicPlanPeriodDto
    {
        public Guid Id { get; set; }

        public int PeriodNumber { get; set; }

        // Required fields
        public string? Topic { get; set; }
        public string? SubTopic { get; set; }
        public decimal? PercentagePlanned { get; set; }
        public DateTime? DatePlanned { get; set; }

        // Completion tracking
        public decimal? PercentageCompleted { get; set; }
        public DateTime? DateCompleted { get; set; }

        // Resources and links
        public string? Resources { get; set; }
        public string? LessonDetailLink { get; set; }
        public string? LessonDetailDescription { get; set; }
        public string? ClassWorkLink { get; set; }
        public string? ClassWorkDescription { get; set; }
        public string? Homework { get; set; }
        public string? HomeworkLink { get; set; }
        public string? HomeworkDescription { get; set; }
        public string? Assessment { get; set; }
        public string? AssessmentLink { get; set; }
        public string? AssessmentDescription { get; set; }
        public string? Notes { get; set; }
    }
}
