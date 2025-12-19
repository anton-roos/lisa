using System;

namespace Lisa.Models.AcademicPlanning
{
    public class AcademicPlanPeriod
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AcademicPlanWeekId { get; set; }
        public int PeriodNumber { get; set; }

        public string? Topic { get; set; }
        public string? Resources { get; set; }
        public string? Assessment { get; set; }
        public string? Homework { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Navigation
        public AcademicPlanWeek AcademicPlanWeek { get; set; } = null!;
    }
}
