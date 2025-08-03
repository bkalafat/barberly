using Barberly.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Barberly.Domain.Tests.Entities;

public class ServiceTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateService()
    {
        // Arrange
        var name = "Haircut";
        var description = "Professional haircut service";
        var price = 25.00m;
        var durationInMinutes = 30;
        var barberShopId = Guid.NewGuid();

        // Act
        var service = Service.Create(name, description, price, durationInMinutes, barberShopId);

        // Assert
        service.Should().NotBeNull();
        service.Id.Should().NotBeEmpty();
        service.Name.Should().Be(name);
        service.Description.Should().Be(description);
        service.Price.Should().Be(price);
        service.DurationInMinutes.Should().Be(durationInMinutes);
        service.BarberShopId.Should().Be(barberShopId);
        service.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var barberShopId = Guid.NewGuid();

        // Act & Assert
        var action = () => Service.Create(invalidName, "Description", 25.00m, 30, barberShopId);
        
        action.Should().Throw<ArgumentException>().WithMessage("Name cannot be empty*");
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrowArgumentException()
    {
        // Arrange
        var barberShopId = Guid.NewGuid();

        // Act & Assert
        var action = () => Service.Create("Haircut", "Description", -10.00m, 30, barberShopId);
        
        action.Should().Throw<ArgumentException>().WithMessage("Price cannot be negative*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithInvalidDuration_ShouldThrowArgumentException(int invalidDuration)
    {
        // Arrange
        var barberShopId = Guid.NewGuid();

        // Act & Assert
        var action = () => Service.Create("Haircut", "Description", 25.00m, invalidDuration, barberShopId);
        
        action.Should().Throw<ArgumentException>().WithMessage("Duration must be positive*");
    }

    [Fact]
    public void Create_WithEmptyBarberShopId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => Service.Create("Haircut", "Description", 25.00m, 30, Guid.Empty);
        
        action.Should().Throw<ArgumentException>().WithMessage("BarberShop ID cannot be empty*");
    }

    [Fact]
    public void UpdateInfo_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        var barberShopId = Guid.NewGuid();
        var service = Service.Create("Haircut", "Description", 25.00m, 30, barberShopId);

        var newName = "Premium Haircut";
        var newDescription = "Premium haircut with styling";
        var newPrice = 35.00m;
        var newDuration = 45;

        // Act
        service.UpdateInfo(newName, newDescription, newPrice, newDuration);

        // Assert
        service.Name.Should().Be(newName);
        service.Description.Should().Be(newDescription);
        service.Price.Should().Be(newPrice);
        service.DurationInMinutes.Should().Be(newDuration);
    }

    [Fact]
    public void UpdateImage_WithValidUrl_ShouldUpdateImageUrl()
    {
        // Arrange
        var barberShopId = Guid.NewGuid();
        var service = Service.Create("Haircut", "Description", 25.00m, 30, barberShopId);
        var imageUrl = "https://example.com/image.jpg";

        // Act
        service.UpdateImage(imageUrl);

        // Assert
        service.ImageUrl.Should().Be(imageUrl);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var barberShopId = Guid.NewGuid();
        var service = Service.Create("Haircut", "Description", 25.00m, 30, barberShopId);
        service.Deactivate(); // First deactivate

        // Act
        service.Activate();

        // Assert
        service.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var barberShopId = Guid.NewGuid();
        var service = Service.Create("Haircut", "Description", 25.00m, 30, barberShopId);

        // Act
        service.Deactivate();

        // Assert
        service.IsActive.Should().BeFalse();
    }
}
