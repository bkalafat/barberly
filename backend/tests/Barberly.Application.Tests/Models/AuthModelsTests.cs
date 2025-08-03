using FluentAssertions;
using Barberly.Api.Models;
using Xunit;

namespace Barberly.Application.Tests.Models;

/// <summary>
/// Tests for authentication models and records
/// </summary>
public class AuthModelsTests
{
    [Fact]
    public void RegisterRequest_ShouldBeImmutable()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var fullName = "Test User";
        var role = "customer";

        // Act
        var request = new RegisterRequest(email, password, fullName, role);

        // Assert
        request.Email.Should().Be(email);
        request.Password.Should().Be(password);
        request.FullName.Should().Be(fullName);
        request.Role.Should().Be(role);
    }

    [Fact]
    public void RegisterRequest_EqualityComparison_ShouldWorkCorrectly()
    {
        // Arrange
        var request1 = new RegisterRequest("test@example.com", "password123", "Test User", "customer");
        var request2 = new RegisterRequest("test@example.com", "password123", "Test User", "customer");
        var request3 = new RegisterRequest("different@example.com", "password123", "Test User", "customer");

        // Act & Assert
        request1.Should().Be(request2);
        request1.Should().NotBe(request3);
        request1.GetHashCode().Should().Be(request2.GetHashCode());
    }

    [Fact]
    public void LoginRequest_ShouldBeImmutable()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";

        // Act
        var request = new LoginRequest(email, password);

        // Assert
        request.Email.Should().Be(email);
        request.Password.Should().Be(password);
    }

    [Fact]
    public void LoginRequest_EqualityComparison_ShouldWorkCorrectly()
    {
        // Arrange
        var request1 = new LoginRequest("test@example.com", "password123");
        var request2 = new LoginRequest("test@example.com", "password123");
        var request3 = new LoginRequest("test@example.com", "different-password");

        // Act & Assert
        request1.Should().Be(request2);
        request1.Should().NotBe(request3);
        request1.GetHashCode().Should().Be(request2.GetHashCode());
    }

    [Fact]
    public void RegisterResponse_ShouldContainValidGuid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var message = "User registered successfully";

        // Act
        var response = new RegisterResponse(userId, message);

        // Assert
        response.UserId.Should().Be(userId);
        response.UserId.Should().NotBe(Guid.Empty);
        response.Message.Should().Be(message);
    }

    [Fact]
    public void LoginResponse_ShouldContainTokenAndMessage()
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

    [Fact]
    public void RegisterUserCommand_ShouldImplementIRequest()
    {
        // Arrange
        var command = new RegisterUserCommand("test@example.com", "password123", "Test User", "customer");

        // Act & Assert
        command.Should().BeAssignableTo<MediatR.IRequest<Guid>>();
        command.Email.Should().Be("test@example.com");
        command.Password.Should().Be("password123");
        command.FullName.Should().Be("Test User");
        command.Role.Should().Be("customer");
    }

    [Fact]
    public void LoginUserCommand_ShouldImplementIRequest()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "password123");

        // Act & Assert
        command.Should().BeAssignableTo<MediatR.IRequest<string>>();
        command.Email.Should().Be("test@example.com");
        command.Password.Should().Be("password123");
    }
}
