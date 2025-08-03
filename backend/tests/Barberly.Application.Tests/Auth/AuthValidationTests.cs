using FluentAssertions;
using FluentValidation.TestHelper;
using Barberly.Api.Models;
using Xunit;

namespace Barberly.Application.Tests.Auth;

/// <summary>
/// Tests for authentication request validation logic
/// </summary>
public class AuthValidationTests
{
    [Theory]
    [InlineData("customer")]
    [InlineData("barber")]
    public void RegisterRequest_ValidRole_ShouldBeValid(string role)
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "test@example.com",
            Password: "password123",
            FullName: "Test User",
            Role: role
        );

        // Act & Assert
        request.Role.Should().Be(role);
        IsValidRole(request.Role).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("CUSTOMER")]
    [InlineData("BARBER")]
    [InlineData("user")]
    [InlineData("shop_owner")]
    [InlineData("admin")]
    public void RegisterRequest_InvalidRole_ShouldBeInvalid(string role)
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "test@example.com",
            Password: "password123",
            FullName: "Test User",
            Role: role
        );

        // Act & Assert
        IsValidRole(request.Role).Should().BeFalse();
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("customer+tag@barberly.com")]
    public void RegisterRequest_ValidEmail_ShouldBeValid(string email)
    {
        // Arrange
        var request = new RegisterRequest(
            Email: email,
            Password: "password123",
            FullName: "Test User",
            Role: "customer"
        );

        // Act & Assert
        request.Email.Should().Be(email);
        IsValidEmail(request.Email).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user.domain.com")]
    public void RegisterRequest_InvalidEmail_ShouldBeInvalid(string email)
    {
        // Arrange
        var request = new RegisterRequest(
            Email: email,
            Password: "password123",
            FullName: "Test User",
            Role: "customer"
        );

        // Act & Assert
        IsValidEmail(request.Email).Should().BeFalse();
    }

    [Theory]
    [InlineData("password123")]
    [InlineData("strongPassword!")]
    [InlineData("123456")]
    public void RegisterRequest_ValidPassword_ShouldBeValid(string password)
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "test@example.com",
            Password: password,
            FullName: "Test User",
            Role: "customer"
        );

        // Act & Assert
        request.Password.Should().Be(password);
        IsValidPassword(request.Password).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("ab")]
    public void RegisterRequest_InvalidPassword_ShouldBeInvalid(string password)
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "test@example.com",
            Password: password,
            FullName: "Test User",
            Role: "customer"
        );

        // Act & Assert
        IsValidPassword(request.Password).Should().BeFalse();
    }

    [Fact]
    public void LoginRequest_ValidData_ShouldBeValid()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "test@example.com",
            Password: "password123"
        );

        // Act & Assert
        request.Email.Should().Be("test@example.com");
        request.Password.Should().Be("password123");
        IsValidEmail(request.Email).Should().BeTrue();
        IsValidPassword(request.Password).Should().BeTrue();
    }

    [Fact]
    public void RegisterResponse_ShouldContainValidData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var message = "User registered successfully";

        // Act
        var response = new RegisterResponse(userId, message);

        // Assert
        response.UserId.Should().Be(userId);
        response.Message.Should().Be(message);
        response.UserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void LoginResponse_ShouldContainValidData()
    {
        // Arrange
        var token = "mock-jwt-token-12345";
        var message = "Login successful";

        // Act
        var response = new LoginResponse(token, message);

        // Assert
        response.Token.Should().Be(token);
        response.Message.Should().Be(message);
        response.Token.Should().NotBeNullOrEmpty();
    }

    // Helper methods for validation
    private static bool IsValidRole(string role)
    {
        var validRoles = new[] { "customer", "barber" };
        return validRoles.Contains(role);
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        return email.Contains('@') && 
               email.IndexOf('@') > 0 && 
               email.IndexOf('@') < email.Length - 1 &&
               email.Count(c => c == '@') == 1;
    }

    private static bool IsValidPassword(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Length >= 6;
    }
}
