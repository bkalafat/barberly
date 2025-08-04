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
}
