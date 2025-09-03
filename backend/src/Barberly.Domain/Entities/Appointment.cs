using Barberly.Domain.Common;

namespace Barberly.Domain.Entities;

public sealed class Appointment : Entity
{
    public Guid UserId { get; private set; }
    public Guid BarberId { get; private set; }
    public Guid ServiceId { get; private set; }
    public DateTimeOffset Start { get; private set; }
    public DateTimeOffset End { get; private set; }
    public string? IdempotencyKey { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    // Cancellation fields
    public bool IsCancelled { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }

    // Concurrency control
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private Appointment() { }

    private Appointment(Guid userId, Guid barberId, Guid serviceId, DateTimeOffset start, DateTimeOffset end, string? idempotencyKey)
    {
        if (userId == Guid.Empty) throw new ArgumentException("userId");
        if (barberId == Guid.Empty) throw new ArgumentException("barberId");
        if (serviceId == Guid.Empty) throw new ArgumentException("serviceId");
        if (start >= end) throw new ArgumentException("start must be before end");

        Id = Guid.NewGuid();
        UserId = userId;
        BarberId = barberId;
        ServiceId = serviceId;
        Start = start;
        End = end;
        IdempotencyKey = idempotencyKey;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        IsCancelled = false;
        RowVersion = Guid.NewGuid().ToByteArray();
    }

    public static Appointment Create(Guid userId, Guid barberId, Guid serviceId, DateTimeOffset start, DateTimeOffset end, string? idempotencyKey = null)
        => new Appointment(userId, barberId, serviceId, start, end, idempotencyKey);

    public void Cancel()
    {
        if (IsCancelled)
            throw new InvalidOperationException("Appointment is already cancelled");

        if (Start <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException("Cannot cancel appointment that has already started");

        IsCancelled = true;
        CancelledAtUtc = DateTimeOffset.UtcNow;
        UpdateRowVersion();
    }

    public void Reschedule(DateTimeOffset newStart, DateTimeOffset newEnd)
    {
        if (IsCancelled)
            throw new InvalidOperationException("Cannot reschedule cancelled appointment");

        if (newStart >= newEnd)
            throw new ArgumentException("New start time must be before end time");

        if (newStart <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Cannot reschedule to a past time");

        Start = newStart;
        End = newEnd;
        UpdateRowVersion();
    }

    private void UpdateRowVersion()
    {
        RowVersion = Guid.NewGuid().ToByteArray();
    }
}
