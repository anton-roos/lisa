using System.Net;
using System.Net.Mail;
using Hangfire;
using Lisa.Models.Entities;

namespace Lisa.Services
{
    public class EmailService(SchoolService schoolService, LearnerService learnerService, ILogger<EmailService> logger)
    {
        private readonly SchoolService _schoolService = schoolService;
        private readonly LearnerService _learnerService = learnerService;
        private readonly ILogger<EmailService> _logger = logger;

        public async Task SendBugReportEmailAsync(BugReport bugReport)
        {
            using var smtpClient = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("portalDCEG@dcegroup.co.za", "Portal@DCEG"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("portalDCEG@dcegroup.co.za"),
                Subject = "Bug Report",
                Body =
                $@"
                    <!DOCTYPE html>
                    <html lang=""en"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Bug Report</title>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px; }}
                            .container {{ max-width: 600px; margin: auto; background: #fff; border: 1px solid #ddd; padding: 20px; border-radius: 8px; }}
                            .header {{ padding: 10px; text-align: center; }}
                            .footer {{ text-align: center; font-size: 12px; color: #666; padding: 10px; }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <div class=""header""><h1>Bug Report</h1></div>
                            <p><strong>Reported At:</strong> {bugReport.ReportedAt}</p>
                            <p><strong>Reported By:</strong> {bugReport.ReportedBy ?? "Anonymous"}</p>
                            <p><strong>User Authenticated:</strong> {(bugReport.UserAuthenticated ? "Yes" : "No")}</p>
                            <p><strong>Page URL:</strong> <a href=""{bugReport.PageUrl}"">{bugReport.PageUrl}</a></p>
                            <p><strong>App Version:</strong> {bugReport.Version}</p>
                            <hr>
                            <p><strong>What Happened:</strong> {bugReport.WhatHappened}</p>
                            <p><strong>What Was Tried:</strong> {bugReport.WhatTried}</p>
                        </div>
                    </body>
                    </html>
                ",
                IsBodyHtml = true
            };

            mailMessage.To.Add("antonroos992@gmail.com");
            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var smtpClient = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("portalDCEG@dcegroup.co.za", "Portal@DCEG"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("portalDCEG@dcegroup.co.za"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);
            await smtpClient.SendMailAsync(mailMessage);
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

            using var smtpClient = new SmtpClient(school.SmtpHost)
            {
                Port = school.SmtpPort,
                Credentials = new NetworkCredential(school.SmtpEmail, school.SmtpPassword),
                EnableSsl = true
            };

            var learner = await _learnerService.GetByIdAsync(learnerId);

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
                    From = new MailAddress(school.SmtpEmail!),
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
