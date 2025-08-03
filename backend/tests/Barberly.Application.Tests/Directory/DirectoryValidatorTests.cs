using Barberly.Application.Directory.Commands;
using FluentValidation.TestHelper;
using Xunit;

namespace Barberly.Application.Tests.Directory;

public class DirectoryValidatorTests
{
    [Fact]
    public void CreateBarberShopCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var validator = new CreateBarberShopCommandValidator();
        var command = new CreateBarberShopCommand(
            "Test Barber Shop",
            "A great barber shop",
            new CreateAddressCommand("123 Main St", "Test City", "Test State", "12345", "Test Country"),
            "555-0123",
            "test@barbershop.com",
            "https://barbershop.com",
            new TimeOnly(9, 0),
            new TimeOnly(18, 0),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
            40.7128m,
            -74.0060m);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateBarberShopCommandValidator_WithEmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new CreateBarberShopCommandValidator();
        var command = new CreateBarberShopCommand(
            "",
            "A great barber shop",
            new CreateAddressCommand("123 Main St", "Test City", "Test State", "12345", "Test Country"),
            "555-0123",
            "test@barbershop.com",
            null,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0),
            new[] { DayOfWeek.Monday },
            40.7128m,
            -74.0060m);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateBarberShopCommandValidator_WithInvalidEmail_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new CreateBarberShopCommandValidator();
        var command = new CreateBarberShopCommand(
            "Test Shop",
            "A great barber shop",
            new CreateAddressCommand("123 Main St", "Test City", "Test State", "12345", "Test Country"),
            "555-0123",
            "invalid-email",
            null,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0),
            new[] { DayOfWeek.Monday },
            40.7128m,
            -74.0060m);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void CreateBarberCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var validator = new CreateBarberCommandValidator();
        var command = new CreateBarberCommand(
            "John Doe",
            "john@barbershop.com",
            "555-0123",
            Guid.NewGuid(),
            5,
            "Experienced barber");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateBarberCommandValidator_WithNegativeExperience_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new CreateBarberCommandValidator();
        var command = new CreateBarberCommand(
            "John Doe",
            "john@barbershop.com",
            "555-0123",
            Guid.NewGuid(),
            -1,
            "Experienced barber");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.YearsOfExperience);
    }

    [Fact]
    public void CreateServiceCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var validator = new CreateServiceCommandValidator();
        var command = new CreateServiceCommand(
            "Haircut",
            "Professional haircut service",
            25.00m,
            30,
            Guid.NewGuid());

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateServiceCommandValidator_WithNegativePrice_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new CreateServiceCommandValidator();
        var command = new CreateServiceCommand(
            "Haircut",
            "Professional haircut service",
            -10.00m,
            30,
            Guid.NewGuid());

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void CreateServiceCommandValidator_WithZeroDuration_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new CreateServiceCommandValidator();
        var command = new CreateServiceCommand(
            "Haircut",
            "Professional haircut service",
            25.00m,
            0,
            Guid.NewGuid());

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationInMinutes);
    }
}
