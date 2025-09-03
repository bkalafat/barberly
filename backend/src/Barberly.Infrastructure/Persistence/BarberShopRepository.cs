using Barberly.Application.Interfaces;
using Barberly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Barberly.Infrastructure.Persistence;

public class BarberShopRepository : IBarberShopRepository
{
    private readonly BarberlyDbContext _db;

    public BarberShopRepository(BarberlyDbContext db)
    {
        _db = db;
    }

    public async Task<BarberShop?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.BarberShops
            .Include(x => x.Barbers)
            .Include(x => x.Services)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(BarberShop shop, CancellationToken ct = default)
    {
        await _db.BarberShops.AddAsync(shop, ct);
    }

    public Task UpdateAsync(BarberShop shop, CancellationToken ct = default)
    {
        _db.BarberShops.Update(shop);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.BarberShops.FindAsync(new object[] { id }, ct);
        if (entity != null)
            _db.BarberShops.Remove(entity);
    }

    public async Task<IReadOnlyList<BarberShop>> GetAllAsync(CancellationToken ct = default)
        => await _db.BarberShops
            .Include(x => x.Barbers)
            .Include(x => x.Services)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BarberShop>> GetNearbyAsync(decimal latitude, decimal longitude, double radiusKm, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        // Haversine formula for distance calculation
        var lat = (double)latitude;
        var lng = (double)longitude;

        var query = _db.BarberShops
            .Include(x => x.Barbers)
            .Include(x => x.Services)
            .Where(x => x.IsActive)
            .Select(x => new
            {
                Shop = x,
                Distance = 6371 * 2 * Math.Asin(Math.Sqrt(
                    Math.Pow(Math.Sin(((lat - (double)x.Latitude) * Math.PI / 180) / 2), 2) +
                    Math.Cos(lat * Math.PI / 180) * Math.Cos((double)x.Latitude * Math.PI / 180) *
                    Math.Pow(Math.Sin(((lng - (double)x.Longitude) * Math.PI / 180) / 2), 2)
                ))
            })
            .Where(x => x.Distance <= radiusKm)
            .OrderBy(x => x.Distance)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Shop);

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BarberShop>> GetFilteredAsync(string? serviceName, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _db.BarberShops
            .Include(x => x.Barbers)
            .Include(x => x.Services)
            .Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(serviceName))
        {
            query = query.Where(x => x.Services.Any(s => s.Name.Contains(serviceName) && s.IsActive));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(x => x.Services.Any(s => s.Price >= minPrice.Value && s.IsActive));
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(x => x.Services.Any(s => s.Price <= maxPrice.Value && s.IsActive));
        }

        return await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }
}
