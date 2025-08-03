using MediatR;

namespace Barberly.Api.Models;

// Request models
public record RegisterRequest(string Email, string Password, string FullName, string Role);
public record LoginRequest(string Email, string Password);

// Response models
public record RegisterResponse(Guid UserId, string Message);
public record LoginResponse(string Token, string Message);

// Internal command wrappers for MediatR
public record RegisterUserCommand(string Email, string Password, string FullName, string Role) : IRequest<Guid>;
public record LoginUserCommand(string Email, string Password) : IRequest<string>;
