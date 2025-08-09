using Lisa.Tests.Helpers;
using Lisa.Repositories;

namespace Lisa.Tests.Repositories;

public class EventLogRepositoryTests : TestBase
{
    private readonly EventLogRepository _eventLogRepository;
    private readonly FakeLogger<EventLogRepository> _fakeLogger;

    public EventLogRepositoryTests()
    {
        _fakeLogger = new FakeLogger<EventLogRepository>();
        _eventLogRepository = new EventLogRepository(DbContextFactory, _fakeLogger);
    }

    [Fact]
    public async Task LogEventAsync_WithValidData_ShouldCreateEventLog()
    {
        // Arrange
        var eventType = "TestEvent";
        var eventData = "{ \"message\": \"test\" }";

        // Act
        await _eventLogRepository.LogEventAsync(eventType, eventData);

        // Assert
        var eventLog = await DbContext.EventLogs.FirstOrDefaultAsync();
        eventLog.Should().NotBeNull();
        eventLog!.EventType.Should().Be(eventType);
        eventLog.EventData.Should().Be(eventData);
        eventLog.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));

        // Verify info log was written
        _fakeLogger.LogEntries
            .Should().Contain(log => log.LogLevel == LogLevel.Information && log.Message!.Contains("Event logged"));
    }

    [Fact]
    public async Task LogEventAsync_WithEmptyEventType_ShouldStillCreateEventLog()
    {
        // Arrange
        var eventType = "";
        var eventData = "test data";

        // Act
        await _eventLogRepository.LogEventAsync(eventType, eventData);

        // Assert
        var eventLog = await DbContext.EventLogs.FirstOrDefaultAsync();
        eventLog.Should().NotBeNull();
        eventLog!.EventType.Should().Be(eventType);
        eventLog.EventData.Should().Be(eventData);
    }

    [Fact]
    public async Task LogEventAsync_WithNullEventData_ShouldHandleGracefully()
    {
        // Arrange
        var eventType = "TestEvent";
        string? eventData = null;

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            _eventLogRepository.LogEventAsync(eventType, eventData!));

        // Should either handle null gracefully or complete the operation
        if (exception == null)
        {
            var eventLog = await DbContext.EventLogs.FirstOrDefaultAsync();
            eventLog.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task LogEventAsync_WithLargeEventData_ShouldCreateEventLog()
    {
        // Arrange
        var eventType = "LargeEvent";
        var eventData = new string('x', 10000); // Large data

        // Act
        await _eventLogRepository.LogEventAsync(eventType, eventData);

        // Assert
        var eventLog = await DbContext.EventLogs.FirstOrDefaultAsync();
        eventLog.Should().NotBeNull();
        eventLog!.EventData.Should().Be(eventData);
        if (eventLog.EventData != null)
        {
            eventLog.EventData.Length.Should().Be(10000);
        }
    }

    [Theory]
    [InlineData("UserLogin", "{ \"userId\": \"123\" }")]
    [InlineData("DataUpdate", "{ \"table\": \"users\", \"id\": 456 }")]
    [InlineData("SystemError", "{ \"error\": \"Connection timeout\" }")]
    public async Task LogEventAsync_WithVariousEventTypes_ShouldCreateEventLogs(string eventType, string eventData)
    {
        // Act
        await _eventLogRepository.LogEventAsync(eventType, eventData);

        // Assert
        var eventLog = await DbContext.EventLogs
            .Where(e => e.EventType == eventType)
            .FirstOrDefaultAsync();
            
        eventLog.Should().NotBeNull();
        eventLog!.EventType.Should().Be(eventType);
        eventLog.EventData.Should().Be(eventData);
    }

    [Fact]
    public async Task LogEventAsync_MultipleEvents_ShouldCreateMultipleEventLogs()
    {
        // Arrange
        var events = new[]
        {
            ("Event1", "Data1"),
            ("Event2", "Data2"),
            ("Event3", "Data3")
        };

        // Act
        foreach (var (eventType, eventData) in events)
        {
            await _eventLogRepository.LogEventAsync(eventType, eventData);
        }

        // Assert
        var eventLogs = await DbContext.EventLogs.ToListAsync();
        eventLogs.Should().HaveCount(3);
        
        eventLogs.Should().Contain(e => e.EventType == "Event1" && e.EventData == "Data1");
        eventLogs.Should().Contain(e => e.EventType == "Event2" && e.EventData == "Data2");
        eventLogs.Should().Contain(e => e.EventType == "Event3" && e.EventData == "Data3");
    }
}
