using System.Net;
using System.Net.Mail;
using Hangfire;

namespace Lisa.Services;

public class EmailService
{
    public void QueueEmail(string to, string subject, string body, int schoolId)
    {
        BackgroundJob.Schedule(() => SendEmail(to, subject, body, schoolId), TimeSpan.FromSeconds(2));
    }

    
    [AutomaticRetry(Attempts = 5, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public void SendEmail(string to, string subject, string body, int schoolId)
    {
        var smtpClient = new SmtpClient("smtp.office365.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("portalDCEG@dcegroup.co.za", "Portal@DCEG"),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("portalDCEG@dcegroup.co.za"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };

        mailMessage.To.Add(to);

        smtpClient.Send(mailMessage);
    }
}