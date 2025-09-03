using Barberly.Domain.Entities;

namespace Barberly.Application.Interfaces;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Service service, CancellationToken ct = default);
    Task UpdateAsync(Service service, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Service>> GetByShopIdAsync(Guid barberShopId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<IReadOnlyList<Service>> GetFilteredAsync(Guid? barberShopId, decimal? minPrice, decimal? maxPrice, int? minDurationMinutes, int? maxDurationMinutes, int page = 1, int pageSize = 20, CancellationToken ct = default);
}
