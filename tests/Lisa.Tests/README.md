# Lisa Tests

This project contains comprehensive unit, integration, and service tests for the Lisa application.

## Test Structure

### Unit Tests
- **Models**: Tests for entity classes and base models
- **Services**: Tests for business logic and service classes
- **Helpers**: Tests for utility and helper classes
- **Enums**: Tests for enumeration types
- **Interfaces**: Tests for interface contracts

### Integration Tests
- **Data**: Database context and entity framework tests
- **Integration**: Full application integration tests

### Test Categories

#### Service Tests
- `EmailServiceTests`: Tests email sending functionality
- `UserServiceTests`: Tests user management operations
- `SchoolServiceTests`: Tests school-related operations
- `EventBusTests`: Tests event publishing and handling

#### Repository Tests
- `EventLogRepositoryTests`: Tests event logging persistence

#### Data Tests
- `LisaDbContextTests`: Tests database operations and relationships

#### Utility Tests
- `TimeHelpersTests`: Tests date/time utility functions
- `EntityTests`: Tests base entity functionality

## Running Tests

### Using .NET CLI
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "ClassName=EmailServiceTests"

# Run tests in parallel
dotnet test --parallel
```

### Using Visual Studio
1. Open Test Explorer (Test → Test Explorer)
2. Build the solution
3. Click "Run All Tests" or select specific tests to run

### Using VS Code
1. Install the .NET Test Explorer extension
2. Open the Test Explorer panel
3. Run tests individually or in groups

## Test Configuration

### Dependencies
- **xUnit**: Primary testing framework
- **FluentAssertions**: Readable assertions
- **Moq**: Mocking framework for dependencies
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing support
- **Microsoft.Extensions.Logging.Testing**: Testing logging infrastructure

### Test Database
Tests use an in-memory Entity Framework database to ensure:
- Fast test execution
- Isolation between tests
- No external dependencies
- Consistent test environments

### Test Patterns
- **Arrange-Act-Assert (AAA)**: Standard test structure
- **Given-When-Then**: Alternative descriptive structure for complex scenarios
- **Theory/InlineData**: Data-driven tests for multiple input scenarios

## Best Practices

1. **Test Independence**: Each test should be independent and not rely on other tests
2. **Descriptive Names**: Test names should clearly describe what is being tested
3. **Single Responsibility**: Each test should verify one specific behavior
4. **Fast Execution**: Tests should run quickly to encourage frequent execution
5. **Readable Assertions**: Use FluentAssertions for clear, readable test failures

## Coverage Goals

Aim for high test coverage across:
- Critical business logic (95%+)
- Service layer operations (90%+)
- Data access patterns (85%+)
- Utility functions (90%+)

## Contributing

When adding new features:
1. Write tests for new functionality
2. Ensure existing tests still pass
3. Maintain or improve code coverage
4. Follow established test patterns and naming conventions

## Troubleshooting

### Common Issues
- **Database conflicts**: Ensure each test uses a unique in-memory database
- **Async operations**: Use proper async/await patterns in tests
- **Mocking issues**: Verify mock setups match actual service dependencies
- **Time-dependent tests**: Use controlled time values instead of DateTime.Now
