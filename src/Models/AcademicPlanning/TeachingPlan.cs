using System;
using System.Collections.Generic;

namespace LISA.Models.AcademicPlanning
{
    public class TeachingPlan
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SchoolId { get; set; }
        public Guid SchoolGradeId { get; set; }
        public int SubjectId { get; set; }
        public Guid TeacherId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Navigation
        public List<AcademicPlanWeek> Weeks { get; set; } = new();
    }
}
