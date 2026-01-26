using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lisa.Data;
using Lisa.Models.AcademicPlanning;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services.AcademicPlanning
{
    public class AcademicLibraryService : IAcademicLibraryService
    {
        private readonly IDbContextFactory<LisaDbContext> _dbFactory;
        private readonly string _documentsPath;

        public AcademicLibraryService(IDbContextFactory<LisaDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _documentsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "AcademicLibrary");
            if (!Directory.Exists(_documentsPath))
            {
                Directory.CreateDirectory(_documentsPath);
            }
        }

        public async Task<AcademicLibraryDocument> UploadDocumentAsync(IFormFile file, DocumentType documentType, Guid schoolId, Guid teacherId, DocumentMetadata metadata, CancellationToken cancellationToken = default)
        {
            // Validate naming convention
            if (!ValidateNamingConvention(documentType, metadata, out var errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            // Get teacher and school names
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var teacher = await db.Users.FindAsync(new object[] { teacherId }, cancellationToken);
            var school = await db.Schools.FindAsync(new object[] { schoolId }, cancellationToken);
            var teacherName = teacher?.Name ?? teacher?.Surname ?? "Unknown";
            var schoolName = school?.ShortName ?? school?.LongName ?? "Unknown";

            // Generate file name
            var fileName = GenerateFileName(documentType, metadata, teacherName, schoolName);
            var fileExtension = Path.GetExtension(file.FileName);
            var fullFileName = $"{fileName}{fileExtension}";

            // Ensure unique file name
            var uniqueFileName = await EnsureUniqueFileNameAsync(fullFileName, cancellationToken);

            // Save file
            var storagePath = Path.Combine(_documentsPath, uniqueFileName);
            await using (var stream = new FileStream(storagePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Create document record
            var document = new AcademicLibraryDocument
            {
                Id = Guid.NewGuid(),
                FileName = uniqueFileName,
                StoragePath = storagePath,
                DocumentType = documentType,
                Year = metadata.Year,
                Grade = metadata.Grade,
                SubjectId = metadata.SubjectId,
                Curriculum = metadata.Curriculum,
                Term = metadata.Term,
                Week = metadata.Week,
                Period = metadata.Period,
                Topic = metadata.Topic,
                SubTopic = metadata.SubTopic,
                Description = metadata.Description,
                AssessmentDate = metadata.AssessmentDate,
                AssessmentType = metadata.AssessmentType,
                SchoolId = schoolId,
                TeacherId = teacherId,
                FileSizeBytes = file.Length,
                MimeType = file.ContentType,
                DescriptionText = metadata.Description,
                CreatedBy = teacherId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await db.Set<AcademicLibraryDocument>().AddAsync(document, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return document;
        }

        public async Task<AcademicLibraryDocument?> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<AcademicLibraryDocument>()
                .AsNoTracking()
                .Include(d => d.Subject)
                .Include(d => d.School)
                .Include(d => d.Teacher)
                .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);
        }

        public async Task<AcademicLibraryDocument> DownloadAsCopyAsync(Guid documentId, Guid newTeacherId, Guid newSchoolId, CancellationToken cancellationToken = default)
        {
            var original = await GetDocumentAsync(documentId, cancellationToken);
            if (original == null)
            {
                throw new ArgumentException("Document not found");
            }

            // Read original file
            if (!File.Exists(original.StoragePath))
            {
                throw new FileNotFoundException("Original document file not found");
            }

            var fileBytes = await File.ReadAllBytesAsync(original.StoragePath, cancellationToken);

            // Get new teacher and school
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var newTeacher = await db.Users.FindAsync(new object[] { newTeacherId }, cancellationToken);
            var newSchool = await db.Schools.FindAsync(new object[] { newSchoolId }, cancellationToken);
            var teacherName = newTeacher?.Name ?? newTeacher?.Surname ?? "Unknown";
            var schoolName = newSchool?.ShortName ?? newSchool?.LongName ?? "Unknown";

            // Generate new file name
            var metadata = new DocumentMetadata
            {
                Year = original.Year,
                Grade = original.Grade,
                SubjectId = original.SubjectId,
                Curriculum = original.Curriculum,
                Term = original.Term,
                Week = original.Week,
                Period = original.Period,
                Topic = original.Topic,
                SubTopic = original.SubTopic,
                Description = original.Description,
                AssessmentDate = original.AssessmentDate,
                AssessmentType = original.AssessmentType
            };

            var fileName = GenerateFileName(original.DocumentType, metadata, teacherName, schoolName);
            var fileExtension = Path.GetExtension(original.FileName);
            var fullFileName = $"{fileName}{fileExtension}";
            var uniqueFileName = await EnsureUniqueFileNameAsync(fullFileName, cancellationToken);

            // Save as new file
            var newStoragePath = Path.Combine(_documentsPath, uniqueFileName);
            await File.WriteAllBytesAsync(newStoragePath, fileBytes, cancellationToken);

            // Create new document record
            var newDocument = new AcademicLibraryDocument
            {
                Id = Guid.NewGuid(),
                FileName = uniqueFileName,
                StoragePath = newStoragePath,
                DocumentType = original.DocumentType,
                Year = original.Year,
                Grade = original.Grade,
                SubjectId = original.SubjectId,
                Curriculum = original.Curriculum,
                Term = original.Term,
                Week = original.Week,
                Period = original.Period,
                Topic = original.Topic,
                SubTopic = original.SubTopic,
                Description = original.Description,
                AssessmentDate = original.AssessmentDate,
                AssessmentType = original.AssessmentType,
                SchoolId = newSchoolId,
                TeacherId = newTeacherId,
                FileSizeBytes = original.FileSizeBytes,
                MimeType = original.MimeType,
                DescriptionText = original.DescriptionText,
                OriginalDocumentId = original.Id,
                CreatedBy = newTeacherId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await db.Set<AcademicLibraryDocument>().AddAsync(newDocument, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return newDocument;
        }

        public async Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var document = await db.Set<AcademicLibraryDocument>().FindAsync(new object[] { documentId }, cancellationToken);
            if (document == null) return false;

            // Only owner can delete
            if (document.TeacherId != userId)
            {
                throw new UnauthorizedAccessException("Only the document owner can delete it");
            }

            // Delete file
            if (File.Exists(document.StoragePath))
            {
                File.Delete(document.StoragePath);
            }

            db.Set<AcademicLibraryDocument>().Remove(document);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateDocumentAsync(AcademicLibraryDocument document, Guid userId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Set<AcademicLibraryDocument>().FindAsync(new object[] { document.Id }, cancellationToken);
            if (existing == null) return false;

            // Only owner can update
            if (existing.TeacherId != userId)
            {
                throw new UnauthorizedAccessException("Only the document owner can update it");
            }

            existing.DescriptionText = document.DescriptionText;
            existing.UpdatedBy = userId;
            existing.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<AcademicLibraryDocument>> GetDocumentsBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<AcademicLibraryDocument>()
                .AsNoTracking()
                .Include(d => d.Subject)
                .Include(d => d.Teacher)
                .Where(d => d.SchoolId == schoolId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AcademicLibraryDocument>> GetAllDocumentsAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<AcademicLibraryDocument>()
                .AsNoTracking()
                .Include(d => d.Subject)
                .Include(d => d.School)
                .Include(d => d.Teacher)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AcademicLibraryDocument>> GetDocumentsByTeacherAsync(Guid teacherId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<AcademicLibraryDocument>()
                .AsNoTracking()
                .Include(d => d.Subject)
                .Include(d => d.School)
                .Where(d => d.TeacherId == teacherId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AcademicLibraryDocument>> GetDocumentsByTypeAsync(DocumentType documentType, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<AcademicLibraryDocument>()
                .AsNoTracking()
                .Include(d => d.Subject)
                .Include(d => d.School)
                .Include(d => d.Teacher)
                .Where(d => d.DocumentType == documentType)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AcademicLibraryDocument>> SearchDocumentsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            var term = searchTerm.ToLower();
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<AcademicLibraryDocument>()
                .AsNoTracking()
                .Include(d => d.Subject)
                .Include(d => d.School)
                .Include(d => d.Teacher)
                .Where(d =>
                    d.FileName.ToLower().Contains(term) ||
                    (d.Topic != null && d.Topic.ToLower().Contains(term)) ||
                    (d.SubTopic != null && d.SubTopic.ToLower().Contains(term)) ||
                    (d.DescriptionText != null && d.DescriptionText.ToLower().Contains(term)) ||
                    (d.Subject != null && d.Subject.Name != null && d.Subject.Name.ToLower().Contains(term)))
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public bool ValidateNamingConvention(DocumentType documentType, DocumentMetadata metadata, out string errorMessage)
        {
            errorMessage = string.Empty;

            switch (documentType)
            {
                case DocumentType.PlanningDocument:
                    if (!metadata.Year.HasValue) { errorMessage = "Year is required for planning documents"; return false; }
                    if (!metadata.Grade.HasValue) { errorMessage = "Grade is required for planning documents"; return false; }
                    if (!metadata.SubjectId.HasValue) { errorMessage = "Subject is required for planning documents"; return false; }
                    if (string.IsNullOrWhiteSpace(metadata.Curriculum)) { errorMessage = "Curriculum is required for planning documents"; return false; }
                    if (!metadata.Term.HasValue) { errorMessage = "Term is required for planning documents"; return false; }
                    if (!metadata.Week.HasValue) { errorMessage = "Week is required for planning documents"; return false; }
                    if (!metadata.Period.HasValue) { errorMessage = "Period is required for planning documents"; return false; }
                    break;

                case DocumentType.SubjectMaterial:
                    if (!metadata.SubjectId.HasValue) { errorMessage = "Subject is required for subject materials"; return false; }
                    if (!metadata.Grade.HasValue) { errorMessage = "Grade is required for subject materials"; return false; }
                    if (string.IsNullOrWhiteSpace(metadata.Topic)) { errorMessage = "Topic is required for subject materials"; return false; }
                    if (string.IsNullOrWhiteSpace(metadata.Description)) { errorMessage = "Description is required for subject materials"; return false; }
                    break;

                case DocumentType.Assessment:
                    if (!metadata.SubjectId.HasValue) { errorMessage = "Subject is required for assessments"; return false; }
                    if (!metadata.Grade.HasValue) { errorMessage = "Grade is required for assessments"; return false; }
                    if (string.IsNullOrWhiteSpace(metadata.Topic)) { errorMessage = "Topic is required for assessments"; return false; }
                    if (string.IsNullOrWhiteSpace(metadata.AssessmentType)) { errorMessage = "Assessment type is required for assessments"; return false; }
                    if (!metadata.AssessmentDate.HasValue) { errorMessage = "Assessment date is required for assessments"; return false; }
                    break;
            }

            return true;
        }

        public string GenerateFileName(DocumentType documentType, DocumentMetadata metadata, string teacherName, string schoolName)
        {
            var sb = new StringBuilder();

            switch (documentType)
            {
                case DocumentType.PlanningDocument:
                    // Format: Year-grade-subject-curriculum-term-week-period-school-teacher
                    sb.Append($"{metadata.Year}-{metadata.Grade}-{metadata.SubjectId}-{metadata.Curriculum}-{metadata.Term}-{metadata.Week}-{metadata.Period}-{schoolName}-{teacherName}");
                    break;

                case DocumentType.SubjectMaterial:
                    // Format: Subject-grade-Topic-SubTopic-description-teacher
                    sb.Append($"{metadata.SubjectId}-{metadata.Grade}-{metadata.Topic}");
                    if (!string.IsNullOrWhiteSpace(metadata.SubTopic))
                    {
                        sb.Append($"-{metadata.SubTopic}");
                    }
                    sb.Append($"-{metadata.Description}-{teacherName}");
                    break;

                case DocumentType.Assessment:
                    // Format: Subject-grade-Topic-subtopic-type-test-date-school-teacher
                    sb.Append($"{metadata.SubjectId}-{metadata.Grade}-{metadata.Topic}");
                    if (!string.IsNullOrWhiteSpace(metadata.SubTopic))
                    {
                        sb.Append($"-{metadata.SubTopic}");
                    }
                    sb.Append($"-{metadata.AssessmentType}-{metadata.AssessmentDate:yyyyMMdd}-{schoolName}-{teacherName}");
                    break;
            }

            // Sanitize file name
            var fileName = sb.ToString();
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            return fileName;
        }

        private async Task<string> EnsureUniqueFileNameAsync(string fileName, CancellationToken cancellationToken)
        {
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var uniqueName = fileName;
            var counter = 1;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            while (await db.Set<AcademicLibraryDocument>()
                .AnyAsync(d => d.FileName == uniqueName, cancellationToken))
            {
                uniqueName = $"{baseName}_{counter}{extension}";
                counter++;
            }

            return uniqueName;
        }
    }
}