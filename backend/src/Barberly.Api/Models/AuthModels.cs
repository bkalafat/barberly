using MediatR;

namespace Barberly.Api.Models;

// Request models
public record RegisterRequest(string Email, string Password, string FullName, string Role);
public record LoginRequest(string Email, string Password);

// Response models
public record RegisterResponse(Guid UserId, string Message);
public record LoginResponse(string Token, string Message);

// Demo models
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Internal command wrappers for MediatR
internal record RegisterUserCommand(string Email, string Password, string FullName, string Role) : IRequest<Guid>;
internal record LoginUserCommand(string Email, string Password) : IRequest<string>;
