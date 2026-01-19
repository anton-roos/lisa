using System;
using System.Collections.Generic;
using Lisa.Enums;
using ClosedXML.Excel;

namespace Lisa.Models.AcademicPlanning
{
    public class TeachingPlan
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SchoolId { get; set; }
        public Guid SchoolGradeId { get; set; }
        public int SubjectId { get; set; }
        public Guid TeacherId { get; set; }
        public Guid AcademicYearId { get; set; } // Added Year selection
        public int Term { get; set; } // Added Term (1-4)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        public AcademicPlanStatus Status { get; set; } = AcademicPlanStatus.Draft;

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }

        public int CurrentVersion { get; set; } = 1;
        public bool IsLocked { get; set; } = false;

        // Catch-up plan support
        public Guid? OriginalPlanId { get; set; } // Reference to original plan if this is a catch-up
        public bool IsCatchUpPlan { get; set; } = false;

        public ICollection<AcademicPlanWeek> Weeks { get; set; } = new List<AcademicPlanWeek>();
    }
}