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

    public async Task<IReadOnlyList<Service>> GetByShopIdAsync(Guid barberShopId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        => await _db.Services
            .Include(x => x.BarberShop)
            .Include(x => x.Barbers)
            .Where(x => x.BarberShopId == barberShopId && x.IsActive)
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Service>> GetFilteredAsync(Guid? barberShopId, decimal? minPrice, decimal? maxPrice, int? minDurationMinutes, int? maxDurationMinutes, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _db.Services
            .Include(x => x.BarberShop)
            .Include(x => x.Barbers)
            .Where(x => x.IsActive);

        if (barberShopId.HasValue)
        {
            query = query.Where(x => x.BarberShopId == barberShopId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(x => x.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= maxPrice.Value);
        }

        if (minDurationMinutes.HasValue)
        {
            query = query.Where(x => x.DurationInMinutes >= minDurationMinutes.Value);
        }

        if (maxDurationMinutes.HasValue)
        {
            query = query.Where(x => x.DurationInMinutes <= maxDurationMinutes.Value);
        }

        return await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }
}
