using Barberly.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Barberly.Domain.Tests.Entities;

public class BarberTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateBarber()
    {
        // Arrange
        var fullName = "John Doe";
        var email = "john@barbershop.com";
        var phone = "555-0123";
        var barberShopId = Guid.NewGuid();
        var yearsOfExperience = 5;
        var bio = "Experienced barber";

        // Act
        var barber = Barber.Create(fullName, email, phone, barberShopId, yearsOfExperience, bio);

        // Assert
        barber.Should().NotBeNull();
        barber.Id.Should().NotBeEmpty();
        barber.FullName.Should().Be(fullName);
        barber.Email.Should().Be(email);
        barber.Phone.Should().Be(phone);
        barber.BarberShopId.Should().Be(barberShopId);
        barber.YearsOfExperience.Should().Be(yearsOfExperience);
        barber.Bio.Should().Be(bio);
        barber.IsActive.Should().BeTrue();
        barber.AverageRating.Should().Be(0);
        barber.TotalReviews.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidFullName_ShouldThrowArgumentException(string invalidFullName)
    {
        // Arrange
        var barberShopId = Guid.NewGuid();

        // Act & Assert
        var action = () => Barber.Create(invalidFullName, "test@test.com", "555-0123", barberShopId);
        
        action.Should().Throw<ArgumentException>().WithMessage("Full name cannot be empty*");
    }

    [Fact]
    public void Create_WithEmptyBarberShopId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => Barber.Create("John Doe", "test@test.com", "555-0123", Guid.Empty);
        
        action.Should().Throw<ArgumentException>().WithMessage("BarberShop ID cannot be empty*");
    }

    [Fact]
    public void Create_WithNegativeExperience_ShouldThrowArgumentException()
    {
        // Arrange
        var barberShopId = Guid.NewGuid();

        // Act & Assert
        var action = () => Barber.Create("John Doe", "test@test.com", "555-0123", barberShopId, -1);
        
        action.Should().Throw<ArgumentException>().WithMessage("Years of experience cannot be negative*");
    }

    [Fact]
    public void UpdateInfo_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        var barberShopId = Guid.NewGuid();
        var barber = Barber.Create("John Doe", "john@test.com", "555-0123", barberShopId);

        var newFullName = "Jane Smith";
        var newEmail = "jane@test.com";
        var newPhone = "555-9999";
        var newBio = "Updated bio";

        // Act
        barber.UpdateInfo(newFullName, newEmail, newPhone, newBio);

        // Assert
        barber.FullName.Should().Be(newFullName);
        barber.Email.Should().Be(newEmail);
        barber.Phone.Should().Be(newPhone);
        barber.Bio.Should().Be(newBio);
    }

    [Fact]
    public void UpdateExperience_WithValidValue_ShouldUpdateExperience()
    {
        // Arrange
        var barberShopId = Guid.NewGuid();
        var barber = Barber.Create("John Doe", "john@test.com", "555-0123", barberShopId, 5);

        // Act
        barber.UpdateExperience(10);

        // Assert
        barber.YearsOfExperience.Should().Be(10);
    }

    [Fact]
    public void UpdateExperience_WithNegativeValue_ShouldThrowArgumentException()
    {
        // Arrange
        var barberShopId = Guid.NewGuid();
        var barber = Barber.Create("John Doe", "john@test.com", "555-0123", barberShopId);

        // Act & Assert
        var action = () => barber.UpdateExperience(-1);
        
        action.Should().Throw<ArgumentException>().WithMessage("Years of experience cannot be negative*");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var barberShopId = Guid.NewGuid();
        var barber = Barber.Create("John Doe", "john@test.com", "555-0123", barberShopId);
        barber.Deactivate(); // First deactivate

        // Act
        barber.Activate();

        // Assert
        barber.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var barberShopId = Guid.NewGuid();
        var barber = Barber.Create("John Doe", "john@test.com", "555-0123", barberShopId);

        // Act
        barber.Deactivate();

        // Assert
        barber.IsActive.Should().BeFalse();
    }
}
