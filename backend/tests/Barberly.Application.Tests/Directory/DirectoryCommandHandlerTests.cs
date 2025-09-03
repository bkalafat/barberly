using Barberly.Application.Directory.Commands;
using Barberly.Application.Directory.Handlers;
using Barberly.Application.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Barberly.Application.Tests.Directory;

public class DirectoryCommandHandlerTests
{
    [Fact]
    public async Task CreateBarberShopCommandHandler_WithValidCommand_ShouldReturnId()
    {
        // Arrange
        var mockRepository = new Mock<IBarberShopRepository>();
        var handler = new CreateBarberShopCommandHandler(mockRepository.Object);
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
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateBarberCommandHandler_WithValidCommand_ShouldReturnId()
    {
        // Arrange
        var mockRepository = new Mock<IBarberRepository>();
        var handler = new CreateBarberCommandHandler(mockRepository.Object);
        var command = new CreateBarberCommand(
            "John Doe",
            "john@barbershop.com",
            "555-0123",
            Guid.NewGuid(),
            5,
            "Experienced barber");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateServiceCommandHandler_WithValidCommand_ShouldReturnId()
    {
        // Arrange
        var mockRepository = new Mock<IServiceRepository>();
        var handler = new CreateServiceCommandHandler(mockRepository.Object);
        var command = new CreateServiceCommand(
            "Haircut",
            "Professional haircut service",
            25.00m,
            30,
            Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }
}
