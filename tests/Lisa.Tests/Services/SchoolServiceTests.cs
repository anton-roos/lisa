using Lisa.Tests.Helpers;
using Microsoft.AspNetCore.Components.Authorization;

namespace Lisa.Tests.Services;

public class SchoolServiceTests : TestBase
{
    private readonly SchoolService _schoolService;
    private readonly Mock<UiEventService> _mockUiEventService;
    private readonly Mock<UserService> _mockUserService;
    private readonly Mock<AuthenticationStateProvider> _mockAuthStateProvider;
    private readonly FakeLogger<SchoolService> _fakeSchoolLogger;

    public SchoolServiceTests()
    {
        _mockUiEventService = new Mock<UiEventService>();
        _mockUserService = new Mock<UserService>();
        _mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        _fakeSchoolLogger = new FakeLogger<SchoolService>();
        
        _schoolService = new SchoolService(
            DbContextFactory,
            _mockUiEventService.Object,
            _mockUserService.Object,
            _mockAuthStateProvider.Object,
            _fakeSchoolLogger);
    }

    [Fact]
    public async Task SetCurrentSchoolAsync_WithValidSchoolId_ShouldReturnSchool()
    {
        // Arrange
        await SeedDatabaseAsync();
        var school = await DbContext.Schools.FirstAsync();

        // Act
        var result = await _schoolService.SetCurrentSchoolAsync(school.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(school.Id);
        _mockUiEventService.Verify(u => u.PublishAsync(It.IsAny<string>(), It.IsAny<School>()), Times.Once);
    }

    [Fact]
    public async Task SetCurrentSchoolAsync_WithNullSchoolId_ShouldReturnNull()
    {
        // Act
        var result = await _schoolService.SetCurrentSchoolAsync(null);

        // Assert
        result.Should().BeNull();
        _mockUiEventService.Verify(u => u.PublishAsync(It.IsAny<string>(), It.IsAny<School?>()), Times.Once);
    }

    [Fact]
    public async Task SetCurrentSchoolAsync_WithNonExistentSchoolId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentSchoolId = Guid.NewGuid();

        // Act
        var result = await _schoolService.SetCurrentSchoolAsync(nonExistentSchoolId);

        // Assert
        result.Should().BeNull();
        
        // Verify warning was logged
        _fakeSchoolLogger.LogEntries
            .Should().Contain(log => log.LogLevel == LogLevel.Warning);
    }

    [Fact]
    public async Task GetSchoolAsync_WithValidId_ShouldReturnSchool()
    {
        // Arrange
        await SeedDatabaseAsync();
        var school = await DbContext.Schools.FirstAsync();

        // Act
        var result = await _schoolService.GetSchoolAsync(school.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(school.Id);
    }

    [Fact]
    public async Task GetSchoolAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _schoolService.GetSchoolAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }
}
