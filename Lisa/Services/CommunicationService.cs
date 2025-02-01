using Lisa.Models.Entities;
using Lisa.Enums;
using Lisa.Models.EmailModels;

namespace Lisa.Services
{
    public class CommunicationService
    {
        private readonly EmailCampaignService _emailCampaignService;
        private readonly LearnerService _learnerService;
        private readonly UserService _userService;
        private readonly GradeService _gradeService;
        private readonly SubjectService _subjectService;
        private readonly ILogger<CommunicationService> _logger;

        public CommunicationService(
            EmailCampaignService emailCampaignService,
            LearnerService learnerService,
            UserService userService,
            GradeService gradeService,
            SubjectService subjectService,
            ILogger<CommunicationService> logger)
        {
            _emailCampaignService = emailCampaignService;
            _learnerService = learnerService;
            _userService = userService;
            _gradeService = gradeService;
            _subjectService = subjectService;
            _logger = logger;
        }

        /// <summary>
        /// General method to send communication based on the CommunicationRequest.
        /// </summary>
        public async Task<EmailCampaign?> SendCommunicationAsync(CommunicationRequest request)
        {
            try
            {
                List<string> recipientEmails = await GetRecipientEmailsAsync(request);

                if (recipientEmails == null || !recipientEmails.Any())
                {
                    _logger.LogWarning("No valid recipients found for the communication request.");
                    return null;
                }

                var emailCampaign = CreateEmailCampaign(request, recipientEmails);
                var createdCampaign = await _emailCampaignService.CreateAsync(emailCampaign);
                _logger.LogInformation("Email campaign {CampaignId} created successfully.", createdCampaign.Id);

                return createdCampaign;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending communication: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Sends communication to all learners in a selected school.
        /// </summary>
        public async Task<EmailCampaign?> SendToAllLearnersAsync(School selectedSchool, EmailTemplate template)
        {
            var request = new CommunicationRequest
            {
                SchoolId = selectedSchool.Id,
                Audience = Audience.Learners,
                TemplateId = template.Id,
                // Additional parameters as needed
            };

            return await SendCommunicationAsync(request);
        }

        /// <summary>
        /// Sends communication to all staff in a selected school.
        /// </summary>
        public async Task<EmailCampaign?> SendToStaffAsync(School selectedSchool, EmailTemplate template)
        {
            var request = new CommunicationRequest
            {
                SchoolId = selectedSchool.Id,
                Audience = Audience.Staff,
                TemplateId = template.Id,
                // Additional parameters as needed
            };

            return await SendCommunicationAsync(request);
        }

        /// <summary>
        /// Sends communication to all parents in a selected school.
        /// </summary>
        public async Task<EmailCampaign?> SendToParentsAsync(School selectedSchool, EmailTemplate template)
        {
            var request = new CommunicationRequest
            {
                SchoolId = selectedSchool.Id,
                Audience = Audience.Parents,
                TemplateId = template.Id,
                // Additional parameters as needed
            };

            return await SendCommunicationAsync(request);
        }

        /// <summary>
        /// Sends communication to both learners and staff in a selected school.
        /// </summary>
        public async Task<EmailCampaign?> SendToLearnersAndStaffAsync(School selectedSchool, EmailTemplate template)
        {
            var request = new CommunicationRequest
            {
                SchoolId = selectedSchool.Id,
                Audience = Audience.LearnersAndStaff,
                TemplateId = template.Id,
                // Additional parameters as needed
            };

            return await SendCommunicationAsync(request);
        }

        /// <summary>
        /// Sends communication to all learners in a specific grade.
        /// </summary>
        public async Task<EmailCampaign?> SendToGradeAsync(Guid gradeId, EmailTemplate template)
        {
            var request = new CommunicationRequest
            {
                GradeId = gradeId,
                Audience = Audience.Grade,
                TemplateId = template.Id,
                // Additional parameters as needed
            };

            return await SendCommunicationAsync(request);
        }

        /// <summary>
        /// Sends communication to all learners in a specific subject.
        /// </summary>
        public async Task<EmailCampaign?> SendToSubjectAsync(int subjectId, EmailTemplate template)
        {
            var request = new CommunicationRequest
            {
                SubjectId = subjectId,
                Audience = Audience.Subject,
                TemplateId = template.Id,
                // Additional parameters as needed
            };

            return await SendCommunicationAsync(request);
        }

        /// <summary>
        /// Gathers recipient emails based on the CommunicationRequest.
        /// </summary>
        private async Task<List<string>> GetRecipientEmailsAsync(CommunicationRequest request)
        {
            switch (request.Audience)
            {
                case Audience.Learners:
                    return await GetLearnerEmailsAsync(request.SchoolId);

                case Audience.Staff:
                    return await GetStaffEmailsAsync(request.SchoolId);

                case Audience.Parents:
                    return await GetParentEmailsAsync(request.SchoolId);

                case Audience.LearnersAndStaff:
                    var learners = await GetLearnerEmailsAsync(request.SchoolId);
                    var staff = await GetStaffEmailsAsync(request.SchoolId);
                    return learners.Concat(staff).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                case Audience.Grade:
                    return await GetGradeEmailsAsync(request.GradeId.Value);

                case Audience.Subject:
                    return await GetSubjectEmailsAsync(request.SubjectId);

                default:
                    _logger.LogWarning("Unknown audience type: {Audience}", request.Audience);
                    return new List<string>();
            }
        }

        /// <summary>
        /// Retrieves learner emails for a specific school.
        /// </summary>
        private async Task<List<string>> GetLearnerEmailsAsync(Guid? schoolId)
        {
            if (!schoolId.HasValue)
            {
                _logger.LogWarning("School ID is null when retrieving learner emails.");
                return new List<string>();
            }

            var learners = await _learnerService.GetLearnersBySchoolAsync(schoolId.Value);
            var emails = learners
                .Select(l => l.Email)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return emails;
        }

        /// <summary>
        /// Retrieves staff emails for a specific school.
        /// </summary>
        private async Task<List<string>> GetStaffEmailsAsync(Guid? schoolId)
        {
            if (!schoolId.HasValue)
            {
                _logger.LogWarning("School ID is null when retrieving staff emails.");
                return new List<string>();
            }

            var staff = await _userService.GetAllStaffForSchoolAsync(schoolId.Value);
            var emails = staff
                .Select(s => s.Email)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return emails;
        }

        /// <summary>
        /// Retrieves parent emails for learners in a specific school.
        /// </summary>
        private async Task<List<string>> GetParentEmailsAsync(Guid? schoolId)
        {
            if (!schoolId.HasValue)
            {
                _logger.LogWarning("School ID is null when retrieving parent emails.");
                return new List<string>();
            }

            var learners = await _learnerService.GetLearnersBySchoolWithParentsAsync(schoolId.Value);
            var emails = learners
                .Where(l => l.Parents != null && l.Parents.Any())
                .SelectMany(l => l.Parents)
                .SelectMany(p => new[] { p.PrimaryEmail, p.SecondaryEmail })
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return emails;
        }

        /// <summary>
        /// Retrieves learner emails for a specific grade.
        /// </summary>
        private async Task<List<string>> GetGradeEmailsAsync(Guid gradeId)
        {
            var learners = await _learnerService.GetLearnersByGradeAsync(gradeId);
            var emails = learners
                .Select(l => l.Email)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return emails;
        }

        /// <summary>
        /// Retrieves learner emails for a specific subject.
        /// </summary>
        private async Task<List<string>> GetSubjectEmailsAsync(int subjectId)
        {
            var learners = await _learnerService.GetBySubjectIdAsync(subjectId);
            var emails = learners
                .Select(l => l.Email)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return emails;
        }

        /// <summary>
        /// Creates an EmailCampaign object based on the request and recipient emails.
        /// </summary>
        private EmailCampaign CreateEmailCampaign(CommunicationRequest request, List<string> recipientEmails)
        {
            var emailCampaign = new EmailCampaign
            {
                Id = Guid.NewGuid(),
                Name = GenerateCampaignName(request.Audience),
                Description = $"Automated notification to {request.Audience}",
                SubjectLine = request.SubjectLine ?? "Important Update from Your School",
                SenderName = request.SenderName ?? "School Admin",
                SenderEmail = request.SenderEmail ?? "admin@school.com", // Ideally fetched from configuration
                ContentHtml = request.ContentHtml ?? "<p>This is an important message.</p>",
                ContentText = request.ContentText ?? "This is an important message.",
                Status = EmailCampaignStatus.Draft,
                ScheduledAt = DateTime.UtcNow.AddMinutes(1), // Schedule to send shortly
                TrackOpens = true,
                TrackClicks = true,
                StatsSentCount = 0,
                StatsOpenCount = 0,
                StatsClickCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailRecipients = recipientEmails.Select(email => new EmailRecipient
                {
                    Id = Guid.NewGuid(),
                    EmailAddress = email,
                    Status = EmailRecipientStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList()
            };

            return emailCampaign;
        }

        /// <summary>
        /// Generates a campaign name based on the audience and current timestamp.
        /// </summary>
        private string GenerateCampaignName(Audience audience)
        {
            return $"{audience}-Campaign-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }
    }
}
