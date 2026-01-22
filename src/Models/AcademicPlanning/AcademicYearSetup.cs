using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lisa.Models.Entities;

namespace Lisa.Models.AcademicPlanning
{
    /// <summary>
    /// Year Setup per school - defines terms, weeks, holidays, exam dates, administrative days
    /// Managed by System Administrators and visible to all teachers/school management/principals
    /// </summary>
    public class AcademicYearSetup
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SchoolId { get; set; }
        public School? School { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }
        public AcademicYear? AcademicYear { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Terms
        public ICollection<AcademicTerm> Terms { get; set; } = new List<AcademicTerm>();

        // Holidays and non-school days
        public ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();

        // Administrative days
        public ICollection<AdministrativeDay> AdministrativeDays { get; set; } = new List<AdministrativeDay>();

        // Exam dates
        public ICollection<ExamDate> ExamDates { get; set; } = new List<ExamDate>();
    }

    /// <summary>
    /// Academic Term - Term 1-4 with start/end dates and week configuration
    /// Can be configured differently per phases (Foundation, Intermediate, Senior) and Grade 12 separately
    /// </summary>
    public class AcademicTerm
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AcademicYearSetupId { get; set; }
        public AcademicYearSetup? AcademicYearSetup { get; set; }

        [Required]
        [Range(1, 4)]
        public int TermNumber { get; set; } // 1, 2, 3, or 4

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Phase/grade applicability
        public string? ApplicablePhases { get; set; } // JSON array: ["Foundation", "Intermediate", "Senior"]
        public bool IsGrade12Specific { get; set; } = false;

        // Weeks in term
        public ICollection<TermWeek> Weeks { get; set; } = new List<TermWeek>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Week within a term
    /// </summary>
    public class TermWeek
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AcademicTermId { get; set; }
        public AcademicTerm? AcademicTerm { get; set; }

        [Required]
        public int WeekNumber { get; set; } // Week number in the term

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Public holidays or non-school days
    /// </summary>
    public class Holiday
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AcademicYearSetupId { get; set; }
        public AcademicYearSetup? AcademicYearSetup { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Administrative days (e.g., staff development days, parent meetings)
    /// Can be set per week/date
    /// </summary>
    public class AdministrativeDay
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AcademicYearSetupId { get; set; }
        public AcademicYearSetup? AcademicYearSetup { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Exam dates - can be configured per phases
    /// </summary>
    public class ExamDate
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AcademicYearSetupId { get; set; }
        public AcademicYearSetup? AcademicYearSetup { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        // Phase/grade applicability
        public string? ApplicablePhases { get; set; } // JSON array: ["Foundation", "Intermediate", "Senior"]
        public bool IsGrade12Specific { get; set; } = false;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}