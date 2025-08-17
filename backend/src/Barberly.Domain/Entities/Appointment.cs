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
    }

    public static Appointment Create(Guid userId, Guid barberId, Guid serviceId, DateTimeOffset start, DateTimeOffset end, string? idempotencyKey = null)
        => new Appointment(userId, barberId, serviceId, start, end, idempotencyKey);
}
