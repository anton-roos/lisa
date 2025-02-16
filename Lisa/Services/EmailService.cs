using System.Net;
using System.Net.Mail;
using Lisa.Models.Entities;

namespace Lisa.Services
{
    public class EmailService(SchoolService schoolService, ILogger<EmailService> logger)
    {
        private readonly SchoolService _schoolService = schoolService;
        private readonly ILogger<EmailService> _logger = logger;
        public async Task SendBugReportEmailAsync(BugReport bugReport)
        {
            using var smtpClient = new SmtpClient("smtp.office365.com");
            smtpClient.Port = 587;
            smtpClient.Credentials = new NetworkCredential("portalDCEG@dcegroup.co.za", "Portal@DCEG");
            smtpClient.EnableSsl = true;

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

        public async Task SendEmailAsync(string to, string subject, string body, Guid schoolId)
        {
            try
            {
                var school = await _schoolService.GetSchoolAsync(schoolId);

                if (school == null)
                {
                    _logger.LogError("School with ID {schoolId} not found.", schoolId);
                    throw new Exception($"School with ID {schoolId} not found.");
                }

                using var smtpClient = new SmtpClient("smtp.office365.com");
                smtpClient.Port = school.SmtpPort;
                smtpClient.Credentials = new NetworkCredential(school.SmtpEmail, school.SmtpPassword);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(school.SmtpEmail ?? throw new Exception("SMTP email not set.")),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);
                await smtpClient.SendMailAsync(mailMessage);

            }
            catch (SmtpException ex)
            {
                _logger.LogError("Failed to send email to {to}: {ex.Message}", to, ex.Message);
                throw;
            }
        }
    }
}
