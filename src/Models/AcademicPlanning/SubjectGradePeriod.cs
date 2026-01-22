using System;
using System.ComponentModel.DataAnnotations;
using Lisa.Models.Entities;

namespace Lisa.Models.AcademicPlanning
{
    /// <summary>
    /// Number of periods per week per subject per grade
    /// Can be configured differently per school
    /// Managed by System Administrators
    /// </summary>
    public class SubjectGradePeriod
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        [Required]
        public Guid SchoolGradeId { get; set; }
        public SchoolGrade? SchoolGrade { get; set; }

        [Required]
        [Range(1, 40)]
        public int PeriodsPerWeek { get; set; }

        // School applicability - if null, applies to all schools
        public Guid? SchoolId { get; set; }
        public School? School { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}