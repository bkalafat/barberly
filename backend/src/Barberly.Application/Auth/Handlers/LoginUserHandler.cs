using Barberly.Application.Auth.Commands;
using Barberly.Application.Interfaces;
using MediatR;

namespace Barberly.Application.Auth.Handlers;

public sealed class LoginUserHandler : IRequestHandler<LoginUserCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginUserHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedEmail = request.Email.Trim();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (user is null)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        return _tokenService.GenerateToken(user.Email, user.Role, user.Id.ToString());
    }
}

