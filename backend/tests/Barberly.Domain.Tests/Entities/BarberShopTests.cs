using Barberly.Domain.Entities;
using Barberly.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Barberly.Domain.Tests.Entities;

public class BarberShopTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateBarberShop()
    {
        // Arrange
        var name = "Test Barber Shop";
        var description = "A great barber shop";
        var address = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var phone = "555-0123";
        var email = "test@barbershop.com";
        var openTime = new TimeOnly(9, 0);
        var closeTime = new TimeOnly(18, 0);
        var workingDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday };
        var latitude = 40.7128m;
        var longitude = -74.0060m;

        // Act
        var barberShop = BarberShop.Create(name, description, address, phone, email, openTime, closeTime, workingDays, latitude, longitude);

        // Assert
        barberShop.Should().NotBeNull();
        barberShop.Id.Should().NotBeEmpty();
        barberShop.Name.Should().Be(name);
        barberShop.Description.Should().Be(description);
        barberShop.Address.Should().Be(address);
        barberShop.Phone.Should().Be(phone);
        barberShop.Email.Should().Be(email);
        barberShop.OpenTime.Should().Be(openTime);
        barberShop.CloseTime.Should().Be(closeTime);
        barberShop.WorkingDays.Should().BeEquivalentTo(workingDays);
        barberShop.Latitude.Should().Be(latitude);
        barberShop.Longitude.Should().Be(longitude);
        barberShop.IsActive.Should().BeTrue();
        barberShop.AverageRating.Should().Be(0);
        barberShop.TotalReviews.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var address = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var workingDays = new[] { DayOfWeek.Monday };

        // Act & Assert
        var action = () => BarberShop.Create(invalidName, "Description", address, "555-0123", "test@test.com", 
            new TimeOnly(9, 0), new TimeOnly(18, 0), workingDays, 0, 0);
        
        action.Should().Throw<ArgumentException>().WithMessage("Name cannot be empty*");
    }

    [Fact]
    public void Create_WithEmptyWorkingDays_ShouldThrowArgumentException()
    {
        // Arrange
        var address = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var emptyWorkingDays = Array.Empty<DayOfWeek>();

        // Act & Assert
        var action = () => BarberShop.Create("Test Shop", "Description", address, "555-0123", "test@test.com", 
            new TimeOnly(9, 0), new TimeOnly(18, 0), emptyWorkingDays, 0, 0);
        
        action.Should().Throw<ArgumentException>().WithMessage("At least one working day must be specified*");
    }

    [Fact]
    public void UpdateInfo_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        var address = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var workingDays = new[] { DayOfWeek.Monday };
        var barberShop = BarberShop.Create("Test Shop", "Description", address, "555-0123", "test@test.com", 
            new TimeOnly(9, 0), new TimeOnly(18, 0), workingDays, 0, 0);

        var newName = "Updated Shop";
        var newDescription = "Updated Description";
        var newPhone = "555-9999";
        var newEmail = "updated@test.com";
        var newWebsite = "https://updated.com";

        // Act
        barberShop.UpdateInfo(newName, newDescription, newPhone, newEmail, newWebsite);

        // Assert
        barberShop.Name.Should().Be(newName);
        barberShop.Description.Should().Be(newDescription);
        barberShop.Phone.Should().Be(newPhone);
        barberShop.Email.Should().Be(newEmail);
        barberShop.Website.Should().Be(newWebsite);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var address = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var workingDays = new[] { DayOfWeek.Monday };
        var barberShop = BarberShop.Create("Test Shop", "Description", address, "555-0123", "test@test.com", 
            new TimeOnly(9, 0), new TimeOnly(18, 0), workingDays, 0, 0);
        
        barberShop.Deactivate(); // First deactivate

        // Act
        barberShop.Activate();

        // Assert
        barberShop.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var address = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var workingDays = new[] { DayOfWeek.Monday };
        var barberShop = BarberShop.Create("Test Shop", "Description", address, "555-0123", "test@test.com", 
            new TimeOnly(9, 0), new TimeOnly(18, 0), workingDays, 0, 0);

        // Act
        barberShop.Deactivate();

        // Assert
        barberShop.IsActive.Should().BeFalse();
    }
}
