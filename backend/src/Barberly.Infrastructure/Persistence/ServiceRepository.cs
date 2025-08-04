using Barberly.Application.Interfaces;
using Barberly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Barberly.Infrastructure.Persistence;

public class ServiceRepository : IServiceRepository
{
    private readonly BarberlyDbContext _db;

    public ServiceRepository(BarberlyDbContext db)
    {
        _db = db;
    }

    public async Task<Service?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Services
            .Include(x => x.BarberShop)
            .Include(x => x.Barbers)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Service service, CancellationToken ct = default)
    {
        await _db.Services.AddAsync(service, ct);
    }

    public Task UpdateAsync(Service service, CancellationToken ct = default)
    {
        _db.Services.Update(service);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Services.FindAsync(new object[] { id }, ct);
        if (entity != null)
            _db.Services.Remove(entity);
    }

    public async Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken ct = default)
        => await _db.Services
            .Include(x => x.BarberShop)
            .Include(x => x.Barbers)
            .ToListAsync(ct);
}
