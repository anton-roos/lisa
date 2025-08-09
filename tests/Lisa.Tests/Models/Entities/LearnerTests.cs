using Lisa.Models.Entities;
using Lisa.Enums;

namespace Lisa.Tests.Models.Entities;

public class LearnerTests : TestBase
{
    [Fact]
    public void Learner_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var learner = new Learner();

        // Assert
        learner.Id.Should().NotBe(Guid.Empty);
        learner.CreatedAt.Should().Be(default(DateTime));
        learner.UpdatedAt.Should().Be(default(DateTime));
    }

    [Fact]
    public void Learner_ShouldHaveInitializedProperties()
    {
        // Act
        var learner = new Learner();

        // Assert
        learner.Code.Should().BeNull();
        learner.Name.Should().BeNull();
        learner.Surname.Should().BeNull();
        learner.Email.Should().BeNull();
        learner.CellNumber.Should().BeNull();
        learner.IdNumber.Should().BeNull();
        learner.Active.Should().BeFalse();
        learner.RegisterClassId.Should().BeNull();
        learner.SchoolId.Should().NotBe(Guid.Empty); // This has a default value
        learner.Gender.Should().Be(Gender.None); // Enum default
        learner.MedicalTransport.Should().Be(MedicalTransport.None); // Enum default
    }

    [Fact]
    public void Learner_ShouldAllowSettingBasicProperties()
    {
        // Arrange
        var learner = new Learner();
        var schoolId = Guid.NewGuid();
        var registerClassId = Guid.NewGuid();

        // Act
        learner.Name = "John";
        learner.Surname = "Doe";
        learner.Email = "john.doe@example.com";
        learner.Gender = Gender.Male;
        learner.CellNumber = "+1234567890";
        learner.SchoolId = schoolId;
        learner.RegisterClassId = registerClassId;
        learner.Active = true;
        learner.Code = "STU001";
        learner.IdNumber = "1234567890123";

        // Assert
        learner.Name.Should().Be("John");
        learner.Surname.Should().Be("Doe");
        learner.Email.Should().Be("john.doe@example.com");
        learner.Gender.Should().Be(Gender.Male);
        learner.CellNumber.Should().Be("+1234567890");
        learner.SchoolId.Should().Be(schoolId);
        learner.RegisterClassId.Should().Be(registerClassId);
        learner.Active.Should().BeTrue();
        learner.Code.Should().Be("STU001");
        learner.IdNumber.Should().Be("1234567890123");
    }

    [Fact]
    public void Learner_ShouldAllowSettingMedicalProperties()
    {
        // Arrange
        var learner = new Learner();

        // Act
        learner.MedicalAidName = "Discovery Health";
        learner.MedicalAidNumber = "123456789";
        learner.MedicalAidPlan = "Essential";
        learner.Allergies = "Peanuts, Shellfish";
        learner.MedicalAilments = "Asthma";
        learner.MedicalInstructions = "Use inhaler as needed";
        learner.DietaryRequirements = "Vegetarian";
        learner.MedicalTransport = MedicalTransport.PrivateAmbulance;

        // Assert
        learner.MedicalAidName.Should().Be("Discovery Health");
        learner.MedicalAidNumber.Should().Be("123456789");
        learner.MedicalAidPlan.Should().Be("Essential");
        learner.Allergies.Should().Be("Peanuts, Shellfish");
        learner.MedicalAilments.Should().Be("Asthma");
        learner.MedicalInstructions.Should().Be("Use inhaler as needed");
        learner.DietaryRequirements.Should().Be("Vegetarian");
        learner.MedicalTransport.Should().Be(MedicalTransport.PrivateAmbulance);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    [InlineData(Gender.None)]
    public void Learner_Gender_ShouldAcceptValidValues(Gender gender)
    {
        // Arrange
        var learner = new Learner();

        // Act
        learner.Gender = gender;

        // Assert
        learner.Gender.Should().Be(gender);
    }

    [Theory]
    [InlineData(MedicalTransport.None)]
    [InlineData(MedicalTransport.PrivateAmbulance)]
    [InlineData(MedicalTransport.PublicAmbulance)]
    public void Learner_MedicalTransport_ShouldAcceptValidValues(MedicalTransport medicalTransport)
    {
        // Arrange
        var learner = new Learner();

        // Act
        learner.MedicalTransport = medicalTransport;

        // Assert
        learner.MedicalTransport.Should().Be(medicalTransport);
    }

    [Fact]
    public async Task Learner_ShouldBePersistableToDatabase()
    {
        // Arrange
        await SeedDatabaseAsync();
        var school = await DbContext.Schools.FirstAsync();
        
        var learner = new Learner
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Surname = "Learner",
            Email = "test.learner@example.com",
            Gender = Gender.Female,
            SchoolId = school.Id,
            Active = true,
            Code = "TEST001",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        DbContext.Learners.Add(learner);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedLearner = await DbContext.Learners.FindAsync(learner.Id);
        savedLearner.Should().NotBeNull();
        savedLearner!.Name.Should().Be("Test");
        savedLearner.Email.Should().Be("test.learner@example.com");
        savedLearner.Gender.Should().Be(Gender.Female);
        savedLearner.SchoolId.Should().Be(school.Id);
        savedLearner.Active.Should().BeTrue();
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("test+tag@example.co.uk")]
    public void Learner_Email_ShouldAcceptValidFormats(string email)
    {
        // Arrange
        var learner = new Learner();

        // Act
        learner.Email = email;

        // Assert
        learner.Email.Should().Be(email);
    }

    [Fact]
    public void Learner_ShouldHaveNavigationProperties()
    {
        // Arrange & Act
        var learner = new Learner();

        // Assert
        learner.School.Should().BeNull();
        learner.RegisterClass.Should().BeNull();
        learner.Parents.Should().BeNull();
        learner.Results.Should().BeNull();
        learner.CareGroup.Should().BeNull();
        learner.Combination.Should().BeNull();
        learner.LearnerSubjects.Should().BeNull();
        learner.EmailReceipts.Should().BeNull();
    }

    [Fact]
    public void Learner_ShouldAllowSettingTimestamps()
    {
        // Arrange
        var learner = new Learner();
        var now = DateTime.UtcNow;

        // Act
        learner.CreatedAt = now;
        learner.UpdatedAt = now;

        // Assert
        learner.CreatedAt.Should().Be(now);
        learner.UpdatedAt.Should().Be(now);
    }
}
