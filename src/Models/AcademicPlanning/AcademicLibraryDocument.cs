using System;
using System.ComponentModel.DataAnnotations;
using Lisa.Models.Entities;

namespace Lisa.Models.AcademicPlanning
{
    /// <summary>
    /// Academic Library Document
    /// Visible to all teachers from all schools, but only the teacher "owning" the document can make changes
    /// If another teacher wants to add or make changes, they must download and then upload under their school and name
    /// Enforced naming conventions
    /// </summary>
    public class AcademicLibraryDocument
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(500)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string StoragePath { get; set; } = string.Empty; // Path where file is stored

        [Required]
        public DocumentType DocumentType { get; set; }

        // Naming convention components (enforced on save)
        public int? Year { get; set; }
        public int? Grade { get; set; } // SystemGrade.SequenceNumber
        public int? SubjectId { get; set; }
        public Subject? Subject { get; set; }
        public string? Curriculum { get; set; }
        public int? Term { get; set; }
        public int? Week { get; set; }
        public int? Period { get; set; }
        public string? Topic { get; set; }
        public string? SubTopic { get; set; }
        public string? Description { get; set; } // presentation/notes/quiz, etc.
        public DateTime? AssessmentDate { get; set; } // For assessments
        public string? AssessmentType { get; set; } // For assessments

        [Required]
        public Guid SchoolId { get; set; } // Original source school
        public School? School { get; set; }

        [Required]
        public Guid TeacherId { get; set; } // Author teacher
        public User? Teacher { get; set; }

        [MaxLength(1000)]
        public string? DescriptionText { get; set; } // Additional description

        public long FileSizeBytes { get; set; }

        [MaxLength(100)]
        public string? MimeType { get; set; }

        // Metadata
        public int DownloadCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        // For documents uploaded as copies
        public Guid? OriginalDocumentId { get; set; } // If this is a copy/download
        public AcademicLibraryDocument? OriginalDocument { get; set; }
    }

    /// <summary>
    /// Document type determines naming convention
    /// </summary>
    public enum DocumentType
    {
        PlanningDocument = 1, // Format: Year-grade-subject-curriculum-term-week-period-school-teacher
        SubjectMaterial = 2, // Format: Subject-grade-Topic-SubTopic-description-teacher
        Assessment = 3 // Format: Subject-grade-Topic-subtopic-type-test-date-school-teacher
    }
}