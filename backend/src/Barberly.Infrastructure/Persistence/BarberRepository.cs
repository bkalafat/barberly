using Barberly.Application.Interfaces;
using Barberly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Barberly.Infrastructure.Persistence;

public class BarberRepository : IBarberRepository
{
    private readonly BarberlyDbContext _db;

    public BarberRepository(BarberlyDbContext db)
    {
        _db = db;
    }

    public async Task<Barber?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Barbers
            .Include(x => x.BarberShop)
            .Include(x => x.Services)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Barber barber, CancellationToken ct = default)
    {
        await _db.Barbers.AddAsync(barber, ct);
    }

    public Task UpdateAsync(Barber barber, CancellationToken ct = default)
    {
        _db.Barbers.Update(barber);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Barbers.FindAsync(new object[] { id }, ct);
        if (entity != null)
            _db.Barbers.Remove(entity);
    }

    public async Task<IReadOnlyList<Barber>> GetAllAsync(CancellationToken ct = default)
        => await _db.Barbers
            .Include(x => x.BarberShop)
            .Include(x => x.Services)
            .ToListAsync(ct);
}
