using Lisa.Models;

namespace Lisa.Tests.Models;

public class EntityTests
{
    [Fact]
    public void Entity_ShouldHaveDefaultValues()
    {
        // Act
        var entity = new Entity();

        // Assert
        entity.Id.Should().NotBe(Guid.Empty);
        entity.CreatedAt.Should().NotBe(default);
        entity.UpdatedAt.Should().NotBe(default);
        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAt.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
        entity.CreatedBy.Should().BeNull();
        entity.UpdatedBy.Should().BeNull();
    }

    [Fact]
    public void Entity_ShouldAllowSettingProperties()
    {
        // Arrange
        var entity = new Entity();
        var testGuid = Guid.NewGuid();
        var testDate = DateTime.UtcNow;

        // Act
        entity.Id = testGuid;
        entity.CreatedAt = testDate;
        entity.UpdatedAt = testDate;
        entity.IsDeleted = true;
        entity.DeletedAt = testDate;
        entity.DeletedBy = testGuid;
        entity.CreatedBy = testGuid;
        entity.UpdatedBy = testGuid;

        // Assert
        entity.Id.Should().Be(testGuid);
        entity.CreatedAt.Should().Be(testDate);
        entity.UpdatedAt.Should().Be(testDate);
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAt.Should().Be(testDate);
        entity.DeletedBy.Should().Be(testGuid);
        entity.CreatedBy.Should().Be(testGuid);
        entity.UpdatedBy.Should().Be(testGuid);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Entity_IsDeleted_ShouldAcceptBooleanValues(bool isDeleted)
    {
        // Arrange
        var entity = new Entity();

        // Act
        entity.IsDeleted = isDeleted;

        // Assert
        entity.IsDeleted.Should().Be(isDeleted);
    }
}
