using MediatR;
using FluentValidation;

namespace Barberly.Application.Auth.Commands;

public sealed record LoginUserCommand(
    string Email,
    string Password
) : IRequest<string>; // JWT veya token dönecek

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
