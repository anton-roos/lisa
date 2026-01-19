using System;
using System.Collections.Generic;

namespace Lisa.Services.AcademicPlanning.DTOs
{
    public class AcademicPlanDto
    {
        public Guid Id { get; set; }

        public Guid SchoolId { get; set; }
        public Guid SchoolGradeId { get; set; }

        public int SubjectId { get; set; }

        public Guid TeacherId { get; set; }

        public Guid AcademicYearId { get; set; } // Added Year
        public int Term { get; set; } // Added Term

        public bool IsCatchUpPlan { get; set; }
        public Guid? OriginalPlanId { get; set; }

        public List<AcademicPlanWeekDto> Weeks { get; set; } = new();
    }
}
