using Barberly.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Barberly.Domain.Tests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAddress()
    {
        // Arrange
        var street = "123 Main Street";
        var city = "Test City";
        var state = "Test State";
        var postalCode = "12345";
        var country = "Test Country";

        // Act
        var address = Address.Create(street, city, state, postalCode, country);

        // Assert
        address.Should().NotBeNull();
        address.Street.Should().Be(street);
        address.City.Should().Be(city);
        address.State.Should().Be(state);
        address.PostalCode.Should().Be(postalCode);
        address.Country.Should().Be(country);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidStreet_ShouldThrowArgumentException(string invalidStreet)
    {
        // Act & Assert
        var action = () => Address.Create(invalidStreet, "City", "State", "12345", "Country");
        
        action.Should().Throw<ArgumentException>().WithMessage("Street cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidCity_ShouldThrowArgumentException(string invalidCity)
    {
        // Act & Assert
        var action = () => Address.Create("123 Main St", invalidCity, "State", "12345", "Country");
        
        action.Should().Throw<ArgumentException>().WithMessage("City cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidCountry_ShouldThrowArgumentException(string invalidCountry)
    {
        // Act & Assert
        var action = () => Address.Create("123 Main St", "City", "State", "12345", invalidCountry);
        
        action.Should().Throw<ArgumentException>().WithMessage("Country cannot be empty*");
    }

    [Fact]
    public void Create_WithNullOrEmptyStateAndPostalCode_ShouldUseEmptyString()
    {
        // Act
        var address1 = Address.Create("123 Main St", "City", "", "", "Country");
        var address2 = Address.Create("123 Main St", "City", "", "", "Country");

        // Assert
        address1.State.Should().Be("");
        address1.PostalCode.Should().Be("");
        address2.State.Should().Be("");
        address2.PostalCode.Should().Be("");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedAddress()
    {
        // Arrange
        var address = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Be("123 Main St, Test City, Test State 12345, Test Country");
    }

    [Fact]
    public void EqualityComparison_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var address1 = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var address2 = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");

        // Act & Assert
        address1.Should().Be(address2);
        (address1 == address2).Should().BeTrue();
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }

    [Fact]
    public void EqualityComparison_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var address2 = Address.Create("456 Oak Ave", "Test City", "Test State", "12345", "Test Country");

        // Act & Assert
        address1.Should().NotBe(address2);
        (address1 == address2).Should().BeFalse();
    }
}
