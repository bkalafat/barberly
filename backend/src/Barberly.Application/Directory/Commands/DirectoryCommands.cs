using FluentValidation;
using MediatR;

namespace Barberly.Application.Directory.Commands;

// BarberShop Commands
public record CreateBarberShopCommand(
    string Name,
    string Description,
    CreateAddressCommand Address,
    string Phone,
    string Email,
    string? Website,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    DayOfWeek[] WorkingDays,
    decimal Latitude,
    decimal Longitude) : IRequest<Guid>;

public record CreateAddressCommand(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country);

public record UpdateBarberShopCommand(
    Guid Id,
    string Name,
    string Description,
    string Phone,
    string Email,
    string? Website) : IRequest<bool>;

public record DeleteBarberShopCommand(Guid Id) : IRequest<bool>;

// Barber Commands
public record CreateBarberCommand(
    string FullName,
    string Email,
    string Phone,
    Guid BarberShopId,
    int YearsOfExperience,
    string? Bio) : IRequest<Guid>;

public record UpdateBarberCommand(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    string? Bio) : IRequest<bool>;

public record DeleteBarberCommand(Guid Id) : IRequest<bool>;

// Service Commands
public record CreateServiceCommand(
    string Name,
    string Description,
    decimal Price,
    int DurationInMinutes,
    Guid BarberShopId) : IRequest<Guid>;

public record UpdateServiceCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int DurationInMinutes) : IRequest<bool>;

public record DeleteServiceCommand(Guid Id) : IRequest<bool>;

// Validators
public class CreateBarberShopCommandValidator : AbstractValidator<CreateBarberShopCommand>
{
    public CreateBarberShopCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Website).MaximumLength(500);
        RuleFor(x => x.WorkingDays).NotEmpty().Must(days => days.Length > 0);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.Address).NotNull().SetValidator(new CreateAddressCommandValidator());
    }
}

public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).MaximumLength(100);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}

public class CreateBarberCommandValidator : AbstractValidator<CreateBarberCommand>
{
    public CreateBarberCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.BarberShopId).NotEmpty();
        RuleFor(x => x.YearsOfExperience).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Bio).MaximumLength(1000);
    }
}

public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationInMinutes).GreaterThan(0).LessThanOrEqualTo(480); // Max 8 hours
        RuleFor(x => x.BarberShopId).NotEmpty();
    }
}
