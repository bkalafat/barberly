using Barberly.Application.Directory.Queries;
using Barberly.Application.Interfaces;
using Barberly.Application.Models;
using Barberly.Domain.Entities;
using MediatR;
using Barberly.Application.Directory.Mappings;

namespace Barberly.Application.Directory.Handlers;

public class GetBarberShopByIdQueryHandler : IRequestHandler<GetBarberShopByIdQuery, BarberShopDto?>
{
    private readonly IBarberShopRepository _barberShopRepository;

    public GetBarberShopByIdQueryHandler(IBarberShopRepository barberShopRepository)
    {
        _barberShopRepository = barberShopRepository;
    }

    public async Task<BarberShopDto?> Handle(GetBarberShopByIdQuery request, CancellationToken cancellationToken)
    {
        var barberShop = await _barberShopRepository.GetByIdAsync(request.Id, cancellationToken);
        return barberShop?.ToDto();
    }
}

public class GetBarberShopsQueryHandler : IRequestHandler<Barberly.Application.Directory.Queries.GetBarberShopsQuery, List<BarberShopDto>>
{
    private readonly IBarberShopRepository _barberShopRepository;

    public GetBarberShopsQueryHandler(IBarberShopRepository barberShopRepository)
    {
        _barberShopRepository = barberShopRepository;
    }

    public async Task<List<BarberShopDto>> Handle(Barberly.Application.Directory.Queries.GetBarberShopsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<BarberShop> barberShops;

        if (request.Latitude.HasValue && request.Longitude.HasValue && request.RadiusKm.HasValue)
        {
            barberShops = await _barberShopRepository.GetNearbyAsync(
                request.Latitude.Value,
                request.Longitude.Value,
                request.RadiusKm.Value,
                request.Page,
                request.PageSize,
                cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.ServiceName) || request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            barberShops = await _barberShopRepository.GetFilteredAsync(
                request.ServiceName,
                request.MinPrice,
                request.MaxPrice,
                request.Page,
                request.PageSize,
                cancellationToken);
        }
        else
        {
            var allShops = await _barberShopRepository.GetAllAsync(cancellationToken);
            barberShops = allShops
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
        }

        return barberShops.Select(x => x.ToDto()).ToList();
    }
}

public class SearchNearbyBarberShopsQueryHandler : IRequestHandler<SearchNearbyBarberShopsQuery, List<BarberShopDto>>
{
    private readonly IBarberShopRepository _barberShopRepository;

    public SearchNearbyBarberShopsQueryHandler(IBarberShopRepository barberShopRepository)
    {
        _barberShopRepository = barberShopRepository;
    }

    public async Task<List<BarberShopDto>> Handle(SearchNearbyBarberShopsQuery request, CancellationToken cancellationToken)
    {
        var barberShops = await _barberShopRepository.GetNearbyAsync(
            request.Latitude,
            request.Longitude,
            request.RadiusKm,
            request.Page,
            request.PageSize,
            cancellationToken);

        return barberShops.Select(x => x.ToDto()).ToList();
    }
}

public class GetBarberByIdQueryHandler : IRequestHandler<GetBarberByIdQuery, BarberDto?>
{
    private readonly IBarberRepository _barberRepository;

    public GetBarberByIdQueryHandler(IBarberRepository barberRepository)
    {
        _barberRepository = barberRepository;
    }

    public async Task<BarberDto?> Handle(GetBarberByIdQuery request, CancellationToken cancellationToken)
    {
        var barber = await _barberRepository.GetByIdAsync(request.Id, cancellationToken);
        return barber?.ToDto();
    }
}

public class GetBarbersByShopIdQueryHandler : IRequestHandler<GetBarbersByShopIdQuery, List<BarberDto>>
{
    private readonly IBarberRepository _barberRepository;

    public GetBarbersByShopIdQueryHandler(IBarberRepository barberRepository)
    {
        _barberRepository = barberRepository;
    }

    public async Task<List<BarberDto>> Handle(GetBarbersByShopIdQuery request, CancellationToken cancellationToken)
    {
        var barbers = await _barberRepository.GetByShopIdAsync(request.BarberShopId, request.Page, request.PageSize, cancellationToken);
        return barbers.Select(x => x.ToDto()).ToList();
    }
}

public class GetBarbersQueryHandler : IRequestHandler<Barberly.Application.Directory.Queries.GetBarbersQuery, List<BarberDto>>
{
    private readonly IBarberRepository _barberRepository;

    public GetBarbersQueryHandler(IBarberRepository barberRepository)
    {
        _barberRepository = barberRepository;
    }

    public async Task<List<BarberDto>> Handle(Barberly.Application.Directory.Queries.GetBarbersQuery request, CancellationToken cancellationToken)
    {
        var barbers = await _barberRepository.GetFilteredAsync(request.BarberShopId, request.ServiceName, request.Page, request.PageSize, cancellationToken);
        return barbers.Select(x => x.ToDto()).ToList();
    }
}

public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ServiceDto?>
{
    private readonly IServiceRepository _serviceRepository;

    public GetServiceByIdQueryHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<ServiceDto?> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.Id, cancellationToken);
        return service?.ToDto();
    }
}

public class GetServicesByShopIdQueryHandler : IRequestHandler<GetServicesByShopIdQuery, List<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepository;

    public GetServicesByShopIdQueryHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<List<ServiceDto>> Handle(GetServicesByShopIdQuery request, CancellationToken cancellationToken)
    {
        var services = await _serviceRepository.GetByShopIdAsync(request.BarberShopId, request.Page, request.PageSize, cancellationToken);
        return services.Select(x => x.ToDto()).ToList();
    }
}

public class GetServicesQueryHandler : IRequestHandler<Barberly.Application.Directory.Queries.GetServicesQuery, List<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepository;

    public GetServicesQueryHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<List<ServiceDto>> Handle(Barberly.Application.Directory.Queries.GetServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await _serviceRepository.GetFilteredAsync(
            request.BarberShopId,
            request.MinPrice,
            request.MaxPrice,
            request.MinDurationMinutes,
            request.MaxDurationMinutes,
            request.Page,
            request.PageSize,
            cancellationToken);

        return services.Select(x => x.ToDto()).ToList();
    }
}
