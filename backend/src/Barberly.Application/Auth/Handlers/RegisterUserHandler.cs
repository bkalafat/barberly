using MediatR;
using Barberly.Application.Auth.Commands;

namespace Barberly.Application.Auth.Handlers;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // TODO: User kaydı, hash, DB ekleme, B2C entegrasyonu
        // Şimdilik mock
        await Task.Delay(10, cancellationToken);
        return Guid.NewGuid();
    }
}
