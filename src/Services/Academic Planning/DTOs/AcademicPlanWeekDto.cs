using System;
using System.Collections.Generic;

namespace Lisa.Services.AcademicPlanning.DTOs
{
    public class AcademicPlanWeekDto
    {
        public Guid Id { get; set; }

        public int WeekNumber { get; set; }

        // Added missing properties
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? Notes { get; set; }

        public List<AcademicPlanPeriodDto> Periods { get; set; } = new();
    }
}
