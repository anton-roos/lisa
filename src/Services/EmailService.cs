using Lisa.Interfaces;
using Lisa.Models.Entities;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;

namespace Lisa.Services;

public class EmailService(SchoolService schoolService, ILogger<EmailService> logger) : IEmailService
{
    private readonly SchoolService _schoolService = Guard.Against.Null(schoolService, nameof(schoolService));
    private readonly ILogger<EmailService> _logger = Guard.Against.Null(logger, nameof(logger));
    private const int DefaultRetryCount = 3;
    private const int DefaultRetryDelayMs = 2000;
    private const int DefaultTimeoutMs = 60000;

    public async Task SendEmailAsync(string to, string subject, string body, Guid schoolId)
    {
        Guard.Against.NullOrEmpty(to, nameof(to), "Email recipient address cannot be empty");
        Guard.Against.Null(schoolId, nameof(schoolId));

        var retryCount = 0;
        Exception? lastException = null;

        while (retryCount < DefaultRetryCount)
        {
            try
            {
                var school = await _schoolService.GetSchoolAsync(schoolId);
                Guard.Against.Null(school, nameof(school), $"School with ID {schoolId} not found");

                using var smtpClient = CreateSmtpClient(school);
                using var mailMessage = CreateMailMessage(to, subject, body, school);

                _logger.LogInformation(
                    "Attempt {RetryCount}: Sending email from {Sender} ({SenderName}) to {Recipient} with subject '{Subject}' using SMTP server {SmtpHost}:{SmtpPort}",
                    retryCount + 1,
                    mailMessage.From!.Address,
                    mailMessage.From.DisplayName,
                    to,
                    subject,
                    school.SmtpHost,
                    school.SmtpPort);

                using var cts = new CancellationTokenSource(DefaultTimeoutMs);
                await smtpClient.SendMailAsync(mailMessage, cts.Token);

                _logger.LogInformation("Email sent successfully to {Recipient}", to);
                return;
            }
            catch (OperationCanceledException ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Email sending timed out on attempt {RetryCount} to {Recipient}: {Message}",
                    retryCount + 1, to, ex.Message);
            }
            catch (SmtpException ex) when (IsTransientSmtpError(ex))
            {
                lastException = ex;
                _logger.LogWarning(ex, "Transient SMTP error on attempt {RetryCount} sending email to {Recipient}: {Message}",
                    retryCount + 1, to, ex.Message);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Non-transient SMTP error sending email to {Recipient}: {Message}", to, ex.Message);
                throw new EmailSendException($"Failed to send email: {ex.Message}", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid SMTP configuration for email to {Recipient}: {Message}", to, ex.Message);
                throw new EmailConfigurationException($"Email configuration error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {Recipient}: {Message}", to, ex.Message);
                throw new EmailSendException($"Unexpected error sending email: {ex.Message}", ex);
            }

            retryCount++;
            if (retryCount < DefaultRetryCount)
            {
                await Task.Delay(DefaultRetryDelayMs * retryCount);
            }
        }

        _logger.LogError(lastException, "Failed to send email to {Recipient} after {RetryCount} attempts",
            to, DefaultRetryCount);
        throw new EmailSendException($"Failed to send email after {DefaultRetryCount} attempts: {lastException?.Message}", lastException);
    }

    private SmtpClient CreateSmtpClient(School school)
    {
        Guard.Against.Null(school, nameof(school));

        var smtpClient = new SmtpClient
        {
            Host = school.SmtpHost ?? "smtp.office365.com",
            Port = school.SmtpPort,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Timeout = DefaultTimeoutMs
        };

        var username = !string.IsNullOrEmpty(school.SmtpUsername)
            ? school.SmtpUsername
            : school.SmtpEmail;

        Guard.Against.NullOrEmpty(username, nameof(username),
            "SMTP username/email not configured for this school");
        Guard.Against.NullOrEmpty(school.SmtpPassword, nameof(school.SmtpPassword),
            "SMTP password not configured for this school");

        smtpClient.Credentials = new NetworkCredential(username, school.SmtpPassword);
        return smtpClient;
    }

    private MailMessage CreateMailMessage(string to, string subject, string body, School school)
    {
        Guard.Against.Null(school, nameof(school));

        var fromEmailAddress = !string.IsNullOrEmpty(school.FromEmail)
            ? school.FromEmail
            : school.SmtpEmail;

        Guard.Against.NullOrEmpty(fromEmailAddress, nameof(fromEmailAddress),
            $"No from email address configured for school {school.Id}");

        var senderName = !string.IsNullOrEmpty(school.LongName)
            ? school.LongName
            : fromEmailAddress;

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmailAddress, senderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
            BodyEncoding = System.Text.Encoding.UTF8,
            Priority = MailPriority.Normal
        };

        mailMessage.To.Add(to);
        return mailMessage;
    }

    private bool IsTransientSmtpError(SmtpException ex)
    {
        if (ex.InnerException is SocketException)
        {
            return true;
        }

        if (ex.InnerException is IOException ioEx &&
            (ioEx.Message.Contains("closed", StringComparison.OrdinalIgnoreCase) ||
             ioEx.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
             ioEx.Message.Contains("reset", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return ex.StatusCode == SmtpStatusCode.ServiceNotAvailable ||
               ex.StatusCode == SmtpStatusCode.MailboxBusy ||
               ex.StatusCode == SmtpStatusCode.LocalErrorInProcessing ||
               ex.StatusCode == SmtpStatusCode.ExceededStorageAllocation ||
               ex.StatusCode == SmtpStatusCode.InsufficientStorage;
    }
}

public class EmailSendException : Exception
{
    public EmailSendException(string message) : base(message) { }
    public EmailSendException(string message, Exception? innerException) : base(message, innerException) { }
}

public class EmailConfigurationException : Exception
{
    public EmailConfigurationException(string message) : base(message) { }
    public EmailConfigurationException(string message, Exception? innerException) : base(message, innerException) { }
}