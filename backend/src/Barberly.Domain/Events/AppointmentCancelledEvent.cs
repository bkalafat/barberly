using Barberly.Domain.Common;

namespace Barberly.Domain.Events;

/// <summary>
/// Domain event raised when an appointment is cancelled.
/// </summary>
public sealed record AppointmentCancelledEvent : IDomainEvent
{
    public Guid AppointmentId { get; init; }
    public Guid UserId { get; init; }
    public Guid BarberId { get; init; }
    public Guid ServiceId { get; init; }
    public DateTimeOffset Start { get; init; }
    public DateTimeOffset End { get; init; }
    public DateTimeOffset CancelledAt { get; init; }
    public DateTimeOffset OccurredOn { get; init; }

    public AppointmentCancelledEvent(
        Guid appointmentId,
        Guid userId,
        Guid barberId,
        Guid serviceId,
        DateTimeOffset start,
        DateTimeOffset end,
        DateTimeOffset cancelledAt)
    {
        AppointmentId = appointmentId;
        UserId = userId;
        BarberId = barberId;
        ServiceId = serviceId;
        Start = start;
        End = end;
        CancelledAt = cancelledAt;
        OccurredOn = DateTimeOffset.UtcNow;
    }
}
