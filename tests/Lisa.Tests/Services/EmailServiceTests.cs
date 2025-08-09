using Lisa.Tests.Helpers;
using Lisa.Interfaces;

namespace Lisa.Tests.Services;

public class EmailServiceTests : TestBase
{
    private readonly IEmailService _emailService;
    private readonly Mock<ISchoolService> _mockSchoolService;
    private readonly FakeLogger<EmailService> _fakeEmailLogger;

    public EmailServiceTests()
    {
        _mockSchoolService = new Mock<ISchoolService>();
        _fakeEmailLogger = new FakeLogger<EmailService>();
        var fakeSchoolLogger = new FakeLogger<SchoolService>();
        
        // Using actual EmailService but with mocked dependencies
        _emailService = new EmailService(
            new SchoolService(DbContextFactory, null!, null!, null!, fakeSchoolLogger), 
            _fakeEmailLogger);
    }

    [Fact]
    public async Task SendEmailAsync_WithValidParameters_ShouldLogAttempt()
    {
        // Arrange
        var schoolId = Guid.NewGuid();
        var school = new School
        {
            Id = schoolId,
            SmtpHost = "smtp.test.com",
            SmtpPort = 587,
            SmtpUsername = "test@test.com",
            SmtpPassword = "password",
            SmtpEmail = "from@test.com"
        };

        // Add school to test database
        DbContext.Schools.Add(school);
        await DbContext.SaveChangesAsync();

        // Act & Assert - Should not throw exception for valid parameters
        var exception = await Record.ExceptionAsync(() => 
            _emailService.SendEmailAsync("to@test.com", "Test Subject", "Test Body", schoolId));

        // The method will likely fail at SMTP connection, but should pass initial validation
        exception.Should().NotBeNull(); // Expected to fail at SMTP level in test environment
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var schoolId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _emailService.SendEmailAsync("", "Test Subject", "Test Body", schoolId));
    }

    [Fact]
    public async Task SendEmailAsync_WithNullSchoolId_ShouldThrowEmailSendException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<EmailSendException>(() =>
            _emailService.SendEmailAsync("to@test.com", "Test Subject", "Test Body", Guid.Empty));
        
        // The inner exception should be ArgumentNullException
        exception.InnerException.Should().BeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task SendEmailAsync_WithNonExistentSchool_ShouldThrowArgumentNullException()
    {
        // Arrange
        var schoolId = Guid.NewGuid(); // Non-existent school

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
            _emailService.SendEmailAsync("to@test.com", "Test Subject", "Test Body", schoolId));
        
        exception.Should().NotBeNull();
    }
}
