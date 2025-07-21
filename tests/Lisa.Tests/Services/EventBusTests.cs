using Lisa.Tests.Helpers;
using Lisa.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Lisa.Tests.Services;

public class EventBusTests : TestBase
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IEventLogRepository> _mockEventLogRepository;
    private readonly FakeLogger<EventBus> _fakeEventBusLogger;
    private readonly EventBus _eventBus;

    public EventBusTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockScope = new Mock<IServiceScope>();
        _mockEventLogRepository = new Mock<IEventLogRepository>();
        _fakeEventBusLogger = new FakeLogger<EventBus>();

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);

        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(p => p.GetRequiredService<IEventLogRepository>())
            .Returns(_mockEventLogRepository.Object);
        _mockServiceProvider.Setup(p => p.GetRequiredService<ILogger<EventBus>>())
            .Returns(_fakeEventBusLogger);

        _eventBus = new EventBus(_mockServiceProvider.Object);
    }

    [Fact]
    public async Task PublishAsync_WithValidEvent_ShouldLogEvent()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test Message", Id = Guid.NewGuid() };

        _mockEventLogRepository.Setup(r => r.LogEventAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert
        _mockEventLogRepository.Verify(r => r.LogEventAsync("TestEvent", It.IsAny<string>()), Times.Once);
        
        // Verify info log was written
        _fakeEventBusLogger.LogEntries
            .Should().Contain(log => log.LogLevel == LogLevel.Information && log.Message!.Contains("Event published"));
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ShouldHandleGracefully()
    {
        // Arrange
        TestEvent? nullEvent = null;

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _eventBus.PublishAsync(nullEvent!));
        
        // Should handle null gracefully or throw appropriate exception
        exception.Should().NotBeNull(); // Method should complete or throw appropriate exception
    }

    [Fact]
    public async Task PublishAsync_WhenRepositoryThrows_ShouldHandleException()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test", Id = Guid.NewGuid() };
        
        _mockEventLogRepository.Setup(r => r.LogEventAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _eventBus.PublishAsync(testEvent));
        
        // Should handle the exception appropriately
        exception.Should().NotBeNull();
    }

    // Test event class for testing
    public class TestEvent
    {
        public string Message { get; set; } = string.Empty;
        public Guid Id { get; set; }
    }
}
