using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lisa.Models.AcademicPlanning;
using Microsoft.AspNetCore.Http;

namespace Lisa.Services.AcademicPlanning
{
    public interface IAcademicLibraryService
    {
        // Document management
        Task<AcademicLibraryDocument> UploadDocumentAsync(IFormFile file, DocumentType documentType, Guid schoolId, Guid teacherId, DocumentMetadata metadata, CancellationToken cancellationToken = default);
        Task<AcademicLibraryDocument?> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
        Task<AcademicLibraryDocument> DownloadAsCopyAsync(Guid documentId, Guid newTeacherId, Guid newSchoolId, CancellationToken cancellationToken = default);
        Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> UpdateDocumentAsync(AcademicLibraryDocument document, Guid userId, CancellationToken cancellationToken = default);
        
        // Queries
        Task<List<AcademicLibraryDocument>> GetDocumentsBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default);
        Task<List<AcademicLibraryDocument>> GetAllDocumentsAsync(CancellationToken cancellationToken = default); // Visible to all teachers
        Task<List<AcademicLibraryDocument>> GetDocumentsByTeacherAsync(Guid teacherId, CancellationToken cancellationToken = default);
        Task<List<AcademicLibraryDocument>> GetDocumentsByTypeAsync(DocumentType documentType, CancellationToken cancellationToken = default);
        Task<List<AcademicLibraryDocument>> SearchDocumentsAsync(string searchTerm, CancellationToken cancellationToken = default);
        
        // Naming convention enforcement
        bool ValidateNamingConvention(DocumentType documentType, DocumentMetadata metadata, out string errorMessage);
        string GenerateFileName(DocumentType documentType, DocumentMetadata metadata, string teacherName, string schoolName);
    }

    public class DocumentMetadata
    {
        public int? Year { get; set; }
        public int? Grade { get; set; }
        public int? SubjectId { get; set; }
        public string? Curriculum { get; set; }
        public int? Term { get; set; }
        public int? Week { get; set; }
        public int? Period { get; set; }
        public string? Topic { get; set; }
        public string? SubTopic { get; set; }
        public string? Description { get; set; }
        public DateTime? AssessmentDate { get; set; }
        public string? AssessmentType { get; set; }
    }
}