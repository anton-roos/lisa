namespace Lisa.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, Guid schoolId);
}
