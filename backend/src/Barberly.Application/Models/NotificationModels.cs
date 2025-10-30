namespace Barberly.Application.Models;

/// <summary>
/// DTO for User information used in notifications
/// </summary>
public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    string Role);

/// <summary>
/// DTO for Appointment information used in notifications
/// </summary>
public record AppointmentDto(
    Guid Id,
    Guid UserId,
    Guid BarberId,
    Guid ServiceId,
    DateTimeOffset Start,
    DateTimeOffset End,
    bool IsCancelled,
    DateTimeOffset? CancelledAtUtc,
    DateTimeOffset CreatedAtUtc);
