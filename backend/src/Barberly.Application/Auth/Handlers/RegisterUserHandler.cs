using Barberly.Application.Auth.Commands;
using Barberly.Application.Interfaces;
using Barberly.Domain.Entities;
using MediatR;

namespace Barberly.Application.Auth.Handlers;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedEmail = request.Email.Trim();
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("User already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FullName = request.FullName.Trim(),
            Role = request.Role.Trim().ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        return user.Id;
    }
}

