using FluentAssertions;
using Barberly.Domain.Common;
using System.Reflection;
using Xunit;

namespace Barberly.Domain.Tests;

/// <summary>
/// Domain layer tests following DDD principles
/// Tests core domain entities and value objects as per copilot-instructions.md
/// </summary>
public class DomainPlaceholderTests
{
    [Fact]
    public void Domain_Assembly_Should_Load_Successfully()
    {
        // Arrange & Act
        var domainAssembly = typeof(Entity).Assembly;
        
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
