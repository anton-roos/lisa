using System;
using System.Collections.Generic;
using Lisa.Enums;

namespace Lisa.Models.AcademicPlanning
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

        // Lifecycle
#pragma warning disable CS8601 // Possible null reference assignment.
        public AcademicPlanStatus Status { get; set; } = AcademicPlanStatus.Draft;
#pragma warning restore CS8601 // Possible null reference assignment.

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }

        // Versioning & locking
        public int CurrentVersion { get; set; } = 1;
        public bool IsLocked { get; set; } = false;

        // Navigation
        public ICollection<AcademicPlanWeek> Weeks { get; set; } = new List<AcademicPlanWeek>();

        //public List<AcademicPlanWeek> Weeks { get; set; } = new();
    }
} 

