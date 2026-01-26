using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lisa.Models.Entities;

namespace Lisa.Models.AcademicPlanning
{
    /// <summary>
    /// Term Assessment Planner
    /// Managed by Principal + School Management
    /// Teachers can capture marks (if assessments are moved by school management)
    /// </summary>
    public class TermAssessmentPlan
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SchoolId { get; set; }
        public School? School { get; set; }

        [Required]
        public Guid SchoolGradeId { get; set; }
        public SchoolGrade? SchoolGrade { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }
        public AcademicYear? AcademicYear { get; set; }

        [Required]
        [Range(1, 4)]
        public int Term { get; set; } // Term 1-4

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Assessments scheduled for this term
        public ICollection<ScheduledAssessment> ScheduledAssessments { get; set; } = new List<ScheduledAssessment>();
    }

    /// <summary>
    /// Scheduled Assessment - links to Capture Results module
    /// Cannot schedule two tests on the same date
    /// </summary>
    public class ScheduledAssessment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TermAssessmentPlanId { get; set; }
        public TermAssessmentPlan? TermAssessmentPlan { get; set; }

        [Required]
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        [Required]
        [MaxLength(200)]
        public string AssessmentName { get; set; } = string.Empty;

        [Required]
        public AssessmentTypeEnum AssessmentType { get; set; } // SASAMs, Exam, DCEG TEST (TT, ST), etc.

        [Required]
        public DateTime ScheduledDate { get; set; }

        // Week information
        public int? WeekNumber { get; set; }

        // Status tracking
        public bool IsCompleted { get; set; } = false; // Strike through when scheduled
        public bool MarksCaptured { get; set; } = false; // Teacher has captured marks
        public bool IsMarksLate { get; set; } = false; // Marks are late

        // Link to ResultSet (Capture Results module)
        public Guid? ResultSetId { get; set; }
        public ResultSet? ResultSet { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Assessment types as per requirements
    /// </summary>
    public enum AssessmentTypeEnum
    {
        SASAMs = 1,
        Exam = 2,
        DCGETestTT = 3, // DCEG TEST (TT)
        DCGETestST = 4, // DCEG TEST (ST)
        Test = 5,
        Assignment = 6,
        Project = 7,
        Other = 99
    }
}