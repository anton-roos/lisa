using Lisa.Interfaces;

namespace Lisa.Tests.Interfaces;

public class IValidatableTests
{
    [Fact]
    public void IValidatable_ShouldHaveValidateMethod()
    {
        // Arrange
        var interfaceType = typeof(IValidatable);

        // Act
        var validateMethod = interfaceType.GetMethod("Validate");

        // Assert
        validateMethod.Should().NotBeNull();
        validateMethod!.ReturnType.Should().Be(typeof(void));
        validateMethod.GetParameters().Should().BeEmpty();
    }

    [Fact]
    public void TestValidatableImplementation_ShouldImplementInterface()
    {
        // Arrange & Act
        var testValidatable = new TestValidatableClass();

        // Assert
        testValidatable.Should().BeAssignableTo<IValidatable>();
    }

    [Fact]
    public void TestValidatableImplementation_Validate_ShouldExecuteWithoutError()
    {
        // Arrange
        var testValidatable = new TestValidatableClass();

        // Act & Assert
        var exception = Record.Exception(() => testValidatable.Validate());
        exception.Should().BeNull();
    }

    [Fact]
    public void TestValidatableWithException_Validate_ShouldThrowExpectedException()
    {
        // Arrange
        var testValidatable = new TestValidatableWithExceptionClass();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => testValidatable.Validate());
        exception.Message.Should().Be("Validation failed");
    }

    // Test implementations for testing purposes
    private class TestValidatableClass : IValidatable
    {
        public bool IsValidated { get; private set; }

        public void Validate()
        {
            IsValidated = true;
        }
    }

    private class TestValidatableWithExceptionClass : IValidatable
    {
        public void Validate()
        {
            throw new InvalidOperationException("Validation failed");
        }
    }
}
