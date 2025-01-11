using System.Net;
using System.Net.Mail;
using Hangfire;

namespace Lisa.Services
{
    public class EmailService(SchoolService schoolService, LearnerService learnerService, ILogger<EmailService> logger)
    {
        private readonly SchoolService _schoolService = schoolService;
        private readonly LearnerService _learnerService = learnerService;
        private readonly ILogger<EmailService> _logger = logger;

        /// <summary>
        /// Queues a job that will send a progress report email for the given learner.
        /// The mail(s) will be sent to the learner’s parent(s), if they have valid addresses.
        /// </summary>
        public void QueueLearnerProgressFeedbackEmail(Guid schoolId, Guid learnerId, string subject, string body)
        {
            BackgroundJob.Schedule(
                () => SendLearnerProgressFeedbackEmail(schoolId, learnerId, subject, body),
                TimeSpan.FromSeconds(5)
            );
            Thread.Sleep(5000);
        }

        /// <summary>
        /// Actually sends the email(s) for the learner’s parents.
        /// The method is called by Hangfire (hence the parameter signature is serializable).
        /// </summary>
        [AutomaticRetry(Attempts = 5, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task SendLearnerProgressFeedbackEmail(Guid schoolId, Guid learnerId, string subject, string body)
        {
            var school = await _schoolService.GetSchoolAsync(schoolId);
            if (school == null)
            {
                _logger.LogError("School with ID {schoolId} not found.", schoolId);
                throw new Exception($"School with ID {schoolId} not found.");
            }

            if (school.SmtpDetails == null)
            {
                _logger.LogError("SMTP details not found for school with ID {schoolId}.", schoolId);
                throw new Exception($"SMTP details not found for school with ID {schoolId}.");
            }
            
            using var smtpClient = new SmtpClient(school.SmtpDetails.Host)
            {
                Port = school.SmtpDetails.Port,
                Credentials = new NetworkCredential(school.SmtpDetails.Email, school.SmtpDetails.Password),
                EnableSsl = true
            };

            var learner = await _learnerService.GetLearnerWithParentsAsync(learnerId);

            if (learner == null)
            {
                _logger.LogError("Learner with ID {learnerId} not found.", learnerId);
                throw new Exception($"Learner with ID {learnerId} not found.");
            }

            var parents = learner.Parents;

            if (parents == null || parents.Count == 0)
            {
                _logger.LogWarning("No parents found for learner with ID {learnerId}.", learnerId);
                return;
            }

            foreach (var parent in parents)
            {
                var emailsToSend = new List<string>();
                if (!string.IsNullOrWhiteSpace(parent.PrimaryEmail))
                    emailsToSend.Add(parent.PrimaryEmail);

                if (!string.IsNullOrWhiteSpace(parent.SecondaryEmail))
                    emailsToSend.Add(parent.SecondaryEmail);

                if (emailsToSend.Count == 0)
                    continue;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(school.SmtpDetails.Email!),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                foreach (var email in emailsToSend)
                {
                    mailMessage.To.Add(email);
                    // NOT Correct, needs to send individual emails to each parent
                }

                smtpClient.Send(mailMessage);

                _logger.LogInformation("Email sent to parent(s) for learner {learnerId}.", learnerId);
            }
        }
    }
}
