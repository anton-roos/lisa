using System;
using System.Collections.Generic;

namespace Lisa.Models.AcademicPlanning
{
    public class AcademicPlanWeek
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AcademicPlanId { get; set; }
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Navigation
        public TeachingPlan AcademicPlan { get; set; } = null!;
        public List<AcademicPlanPeriod> Periods { get; set; } = new();
    }
}
