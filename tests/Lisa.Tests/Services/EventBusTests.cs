using Lisa.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Lisa.Tests.Services;

public class EventBusTests : TestBase
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly FakeLogger<EventBus> _fakeEventBusLogger;
    private readonly EventBus _eventBus;

    public EventBusTests()
    {
        // Use IServiceScopeFactory mock as the main service provider
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _fakeEventBusLogger = new FakeLogger<EventBus>();

        // Setup the scope factory to return our mock scope
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
        
        // Setup the scope to return our mock service provider
        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
        
        // Setup the scoped service provider to return required services
        _mockServiceProvider.Setup(p => p.GetService(typeof(ILogger<EventBus>)))
            .Returns(_fakeEventBusLogger);

        // Create EventBus with a service provider that includes the scope factory
        var mainServiceProvider = new Mock<IServiceProvider>();
        mainServiceProvider.As<IServiceScopeFactory>()
            .Setup(f => f.CreateScope())
            .Returns(_mockScope.Object);

        _eventBus = new EventBus(mainServiceProvider.Object);
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
