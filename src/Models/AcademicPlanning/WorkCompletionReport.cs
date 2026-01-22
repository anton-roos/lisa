using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lisa.Models.Entities;

namespace Lisa.Models.AcademicPlanning
{
    /// <summary>
    /// Work Completion Report
    /// Shows planned date and % against actual date and %
    /// Automatically sent weekly to selected senior people
    /// Also carried over to QAC Delivery Control section
    /// </summary>
    public class WorkCompletionReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SchoolId { get; set; }
        public School? School { get; set; }

        [Required]
        public Guid TeachingPlanId { get; set; }
        public TeachingPlan? TeachingPlan { get; set; }

        [Required]
        public DateTime ReportDate { get; set; } // Date of the report

        [Required]
        public DateTime PeriodStartDate { get; set; } // Start of reporting period
        [Required]
        public DateTime PeriodEndDate { get; set; } // End of reporting period

        // Summary statistics
        public int TotalPeriods { get; set; }
        public int PlannedPeriods { get; set; }
        public int CompletedPeriods { get; set; }
        public decimal AveragePercentagePlanned { get; set; }
        public decimal AveragePercentageCompleted { get; set; }
        public int PeriodsBehindSchedule { get; set; } // Periods with DateCompleted > DatePlanned

        // Report status
        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Report details
        public ICollection<WorkCompletionReportDetail> Details { get; set; } = new List<WorkCompletionReportDetail>();
    }

    /// <summary>
    /// Individual period detail in work completion report
    /// </summary>
    public class WorkCompletionReportDetail
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkCompletionReportId { get; set; }
        public WorkCompletionReport? WorkCompletionReport { get; set; }

        [Required]
        public Guid AcademicPlanPeriodId { get; set; }
        public AcademicPlanPeriod? AcademicPlanPeriod { get; set; }

        public int WeekNumber { get; set; }
        public int PeriodNumber { get; set; }
        public string? Topic { get; set; }
        public string? SubTopic { get; set; }

        public DateTime? DatePlanned { get; set; }
        public decimal? PercentagePlanned { get; set; }
        public DateTime? DateCompleted { get; set; }
        public decimal? PercentageCompleted { get; set; }

        public bool IsOnSchedule { get; set; } = true; // DateCompleted <= DatePlanned
        public bool IsLate { get; set; } = false; // DateCompleted > DatePlanned
        public int? DaysBehind { get; set; } // Days behind schedule
    }

    /// <summary>
    /// Work Completion Report Recipients - senior people who receive automatic weekly reports
    /// </summary>
    public class WorkCompletionReportRecipient
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SchoolId { get; set; }
        public School? School { get; set; }

        [Required]
        public Guid UserId { get; set; } // Recipient user (Principal, School Management, System Admin)
        public User? User { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}