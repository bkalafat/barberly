using Barberly.Domain.Entities;

namespace Barberly.Application.Interfaces;

public interface IBarberShopRepository
{
    Task<BarberShop?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(BarberShop shop, CancellationToken ct = default);
    Task UpdateAsync(BarberShop shop, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<BarberShop>> GetAllAsync(CancellationToken ct = default);
}
