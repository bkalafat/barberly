using Barberly.Domain.Entities;

namespace Barberly.Application.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Appointment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);
    Task AddAsync(Appointment appt, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetByBarberAndRangeAsync(Guid barberId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default);
}
