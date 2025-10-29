using Barberly.Domain.Common;

namespace Barberly.Domain.Events;

/// <summary>
/// Domain event raised when an appointment is successfully booked.
/// </summary>
public sealed record AppointmentBookedEvent : IDomainEvent
{
    public Guid AppointmentId { get; init; }
    public Guid UserId { get; init; }
    public Guid BarberId { get; init; }
    public Guid ServiceId { get; init; }
    public DateTimeOffset Start { get; init; }
    public DateTimeOffset End { get; init; }
    public DateTimeOffset OccurredOn { get; init; }

    public AppointmentBookedEvent(
        Guid appointmentId,
        Guid userId,
        Guid barberId,
        Guid serviceId,
        DateTimeOffset start,
        DateTimeOffset end)
    {
        AppointmentId = appointmentId;
        UserId = userId;
        BarberId = barberId;
        ServiceId = serviceId;
        Start = start;
        End = end;
        OccurredOn = DateTimeOffset.UtcNow;
    }
}
