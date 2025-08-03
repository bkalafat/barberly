using MediatR;
using Barberly.Application.Auth.Commands;

namespace Barberly.Application.Auth.Handlers;

public sealed class LoginUserHandler : IRequestHandler<LoginUserCommand, string>
{
    public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        // TODO: Kullanıcı doğrulama, şifre kontrolü, JWT üretimi
        // Şimdilik mock
        await Task.Delay(10, cancellationToken);
        return "mock-jwt-token";
    }
}
