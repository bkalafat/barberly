using Barberly.Domain.Entities;

namespace Barberly.Application.Interfaces;

public interface IBarberRepository
{
    Task<Barber?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Barber barber, CancellationToken ct = default);
    Task UpdateAsync(Barber barber, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Barber>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Barber>> GetByShopIdAsync(Guid barberShopId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<IReadOnlyList<Barber>> GetFilteredAsync(Guid? barberShopId, string? serviceName, int page = 1, int pageSize = 20, CancellationToken ct = default);
}
