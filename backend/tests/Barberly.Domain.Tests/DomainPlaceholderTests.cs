using FluentAssertions;
using Xunit;

namespace Barberly.Domain.Tests;

/// <summary>
/// Placeholder test class for Domain layer tests.
/// Will be expanded when domain entities are implemented.
/// </summary>
public class DomainPlaceholderTests
{
    [Fact]
    public void Domain_Layer_Should_Exist()
    {
        // Arrange & Act
        var domainAssembly = typeof(Barberly.Domain.AssemblyReference).Assembly;
        
        // Assert
        domainAssembly.Should().NotBeNull();
        domainAssembly.GetName().Name.Should().Be("Barberly.Domain");
    }

    [Theory]
    [InlineData("customer")]
    [InlineData("barber")]
    [InlineData("shop_owner")]
    [InlineData("admin")]
    public void UserRole_Values_Should_Be_Valid(string role)
    {
        // Arrange & Act
        var validRoles = new[] { "customer", "barber", "shop_owner", "admin" };
        
        // Assert
        validRoles.Should().Contain(role);
    }

    [Fact]
    public void Email_Should_Have_Valid_Format()
    {
        // Arrange
        var validEmails = new[]
        {
            "user@example.com",
            "test.user@domain.co.uk",
            "customer+tag@barberly.com"
        };

        var invalidEmails = new[]
        {
            "invalid-email",
            "@domain.com",
            "user@",
            "user.domain.com"
        };

        // Act & Assert
        foreach (var email in validEmails)
        {
            IsValidEmail(email).Should().BeTrue($"'{email}' should be valid");
        }

        foreach (var email in invalidEmails)
        {
            IsValidEmail(email).Should().BeFalse($"'{email}' should be invalid");
        }
    }

    private static bool IsValidEmail(string email)
    {
        return email.Contains('@') && 
               email.IndexOf('@') > 0 && 
               email.IndexOf('@') < email.Length - 1 &&
               email.Count(c => c == '@') == 1;
    }
}
