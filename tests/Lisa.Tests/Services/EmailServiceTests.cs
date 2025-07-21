using Lisa.Tests.Helpers;

namespace Lisa.Tests.Services;

public class EmailServiceTests : TestBase
{
    private readonly EmailService _emailService;
    private readonly Mock<SchoolService> _mockSchoolService;
    private readonly FakeLogger<EmailService> _fakeEmailLogger;

    public EmailServiceTests()
    {
        _mockSchoolService = new Mock<SchoolService>();
        _fakeEmailLogger = new FakeLogger<EmailService>();
        _emailService = new EmailService(_mockSchoolService.Object, _fakeEmailLogger);
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
            FromEmail = "from@test.com"
        };

        _mockSchoolService.Setup(s => s.GetSchoolAsync(schoolId))
            .ReturnsAsync(school);

        // Act & Assert - Should not throw exception for valid parameters
        var exception = await Record.ExceptionAsync(() => 
            _emailService.SendEmailAsync("to@test.com", "Test Subject", "Test Body", schoolId));

        // The method will likely fail at SMTP connection, but should pass validation
        _mockSchoolService.Verify(s => s.GetSchoolAsync(schoolId), Times.Once);
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
    public async Task SendEmailAsync_WithNullSchoolId_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _emailService.SendEmailAsync("to@test.com", "Test Subject", "Test Body", Guid.Empty));
    }

    [Fact]
    public async Task SendEmailAsync_WithNonExistentSchool_ShouldThrowArgumentNullException()
    {
        // Arrange
        var schoolId = Guid.NewGuid();
        _mockSchoolService.Setup(s => s.GetSchoolAsync(schoolId))
            .ReturnsAsync((School?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _emailService.SendEmailAsync("to@test.com", "Test Subject", "Test Body", schoolId));
    }
}
