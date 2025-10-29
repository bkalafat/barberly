using Barberly.Application.Auth.Commands;
using Barberly.Application.Auth.Handlers;
using Barberly.Application.Interfaces;
using Barberly.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Barberly.Application.Tests.Auth;

public class AuthHandlersTests
{
    [Fact]
    public async Task RegisterUserHandler_ShouldCreateUser_WhenEmailIsAvailable()
    {
        // Arrange
        var userRepoMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var hashedPassword = "hashed-password";
        User? persistedUser = null;

        userRepoMock
            .Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync((User?)null);

        passwordHasherMock
            .Setup(h => h.HashPassword("Password123!"))
            .Returns(hashedPassword);

        userRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => persistedUser = u)
            .Returns(Task.CompletedTask);

        var handler = new RegisterUserHandler(userRepoMock.Object, passwordHasherMock.Object);
        var command = new RegisterUserCommand(" test@example.com ", "Password123!", " Test User ", "Customer");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        persistedUser.Should().NotBeNull();
        persistedUser!.Id.Should().Be(result);
        persistedUser.Email.Should().Be("test@example.com");
        persistedUser.FullName.Should().Be("Test User");
        persistedUser.Role.Should().Be("customer");
        persistedUser.PasswordHash.Should().Be(hashedPassword);

        userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        passwordHasherMock.Verify(h => h.HashPassword("Password123!"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserHandler_ShouldThrow_WhenUserAlreadyExists()
    {
        // Arrange
        var userRepoMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        userRepoMock
            .Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(new User { Email = "test@example.com" });

        var handler = new RegisterUserHandler(userRepoMock.Object, passwordHasherMock.Object);
        var command = new RegisterUserCommand("test@example.com", "Password123!", "Test User", "customer");

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User already exists.");
        userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        passwordHasherMock.Verify(h => h.HashPassword(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginUserHandler_ShouldReturnToken_WhenCredentialsValid()
    {
        // Arrange
        var userRepoMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var tokenServiceMock = new Mock<ITokenService>();
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashed",
            Role = "customer"
        };

        userRepoMock
            .Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);

        passwordHasherMock
            .Setup(h => h.VerifyPassword("Password123!", "hashed"))
            .Returns(true);

        tokenServiceMock
            .Setup(t => t.GenerateToken(existingUser.Email, existingUser.Role, existingUser.Id.ToString()))
            .Returns("token");

        var handler = new LoginUserHandler(userRepoMock.Object, passwordHasherMock.Object, tokenServiceMock.Object);
        var command = new LoginUserCommand(" test@example.com ", "Password123!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("token");

        tokenServiceMock.Verify(t => t.GenerateToken(existingUser.Email, existingUser.Role, existingUser.Id.ToString()), Times.Once);
    }

    [Fact]
    public async Task LoginUserHandler_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var userRepoMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var tokenServiceMock = new Mock<ITokenService>();

        userRepoMock
            .Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync((User?)null);

        var handler = new LoginUserHandler(userRepoMock.Object, passwordHasherMock.Object, tokenServiceMock.Object);
        var command = new LoginUserCommand("test@example.com", "Password123!");

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid credentials.");

        passwordHasherMock.Verify(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        tokenServiceMock.Verify(t => t.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginUserHandler_ShouldThrow_WhenPasswordInvalid()
    {
        // Arrange
        var userRepoMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var tokenServiceMock = new Mock<ITokenService>();
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashed",
            Role = "customer"
        };

        userRepoMock
            .Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);

        passwordHasherMock
            .Setup(h => h.VerifyPassword("Password123!", "hashed"))
            .Returns(false);

        var handler = new LoginUserHandler(userRepoMock.Object, passwordHasherMock.Object, tokenServiceMock.Object);
        var command = new LoginUserCommand("test@example.com", "Password123!");

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid credentials.");

        tokenServiceMock.Verify(t => t.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}

