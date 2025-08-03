using MediatR;
using FluentValidation;

namespace Barberly.Application.Auth.Commands;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FullName,
    string Role // "customer" veya "barber"
) : IRequest<Guid>;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FullName).NotEmpty();
        RuleFor(x => x.Role).NotEmpty().Must(r => r == "customer" || r == "barber");
    }
}
