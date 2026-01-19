using System;
using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.AcademicPlanning
{
    public class AcademicPlanPeriod
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AcademicPlanWeekId { get; set; }
        public AcademicPlanWeek AcademicPlanWeek { get; set; } = null!;

        public int PeriodNumber { get; set; } 
        
        // Required fields
        [Required]
        [MaxLength(200)]
        public string? Topic { get; set; } // Lesson topic
        
        [MaxLength(200)]
        public string? SubTopic { get; set; } // Sub-topic
        
        [Range(0, 100)]
        public decimal? PercentagePlanned { get; set; } // % planned
        
        public DateTime? DatePlanned { get; set; } // Date planned for
        
        // Completion tracking
        [Range(0, 100)]
        public decimal? PercentageCompleted { get; set; } // % completed
        
        public DateTime? DateCompleted { get; set; } // Actual date completed
        
        // Resources and links
        public string? Resources { get; set; } // Resources/class work
        public string? LessonDetailLink { get; set; } // Link to document/PowerPoint in Academic Library
        public string? LessonDetailDescription { get; set; } // Description for lesson detail link
        
        public string? ClassWorkLink { get; set; } // Link to class work document
        public string? ClassWorkDescription { get; set; } // Description for class work link
        
        public string? Homework { get; set; } // Homework
        public string? HomeworkLink { get; set; } // Link to homework document
        public string? HomeworkDescription { get; set; } // Description for homework link
        
        public string? Assessment { get; set; } // Assessment
        public string? AssessmentLink { get; set; } // Link to assessment document
        public string? AssessmentDescription { get; set; } // Description for assessment link
        
        // Notes column for teacher
        [MaxLength(1000)]
        public string? Notes { get; set; } // Notes at planning stage or after lesson
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}