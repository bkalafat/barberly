using Barberly.Application.Interfaces;
using Barberly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Barberly.Infrastructure.Persistence;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly BarberlyDbContext _db;

    public AppointmentRepository(BarberlyDbContext db) { _db = db; }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Appointment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
        => await _db.Appointments.FirstOrDefaultAsync(a => a.IdempotencyKey == idempotencyKey, ct);

    public async Task AddAsync(Appointment appt, CancellationToken ct = default)
    {
        await _db.Appointments.AddAsync(appt, ct);
    }

    public async Task<IReadOnlyList<Appointment>> GetByBarberAndRangeAsync(Guid barberId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default)
        => await _db.Appointments.Where(a => a.BarberId == barberId && a.Start < end && a.End > start).ToListAsync(ct);
}
