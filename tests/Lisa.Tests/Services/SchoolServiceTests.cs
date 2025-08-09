using Lisa.Tests.Helpers;
using Lisa.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace Lisa.Tests.Services;

public class SchoolServiceTests : TestBase
{
    private readonly ISchoolService _schoolService;
    private readonly Mock<AuthenticationStateProvider> _mockAuthStateProvider;
    private readonly FakeLogger<SchoolService> _fakeSchoolLogger;

    public SchoolServiceTests()
    {
        _mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        _fakeSchoolLogger = new FakeLogger<SchoolService>();
        
        // Create actual service with mocked dependencies
        _schoolService = new SchoolService(
            DbContextFactory,
            new UiEventService(new FakeLogger<UiEventService>()),
            // We'll create a minimal UserService for testing
            null!, // UserService will need to be handled separately
            _mockAuthStateProvider.Object,
            _fakeSchoolLogger);
    }



    [Fact]
    public async Task SetCurrentSchoolAsync_WithNullSchoolId_ShouldReturnNull()
    {
        // Act
        var result = await _schoolService.SetCurrentSchoolAsync(null);

        // Assert
        result.Should().BeNull();
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
