using Barberly.Application.Models;
using MediatR;

namespace Barberly.Application.Directory.Queries;

// BarberShop Queries
public record GetBarberShopByIdQuery(Guid Id) : IRequest<BarberShopDto?>;

public record GetBarberShopsQuery(
    decimal? Latitude,
    decimal? Longitude,
    double? RadiusKm,
    string? ServiceName,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20) : IRequest<List<BarberShopDto>>;

public record SearchNearbyBarberShopsQuery(
    decimal Latitude,
    decimal Longitude,
    double RadiusKm = 10.0,
    int Page = 1,
    int PageSize = 20) : IRequest<List<BarberShopDto>>;

// Barber Queries
public record GetBarberByIdQuery(Guid Id) : IRequest<BarberDto?>;

public record GetBarbersByShopIdQuery(
    Guid BarberShopId,
    int Page = 1,
    int PageSize = 20) : IRequest<List<BarberDto>>;

public record GetBarbersQuery(
    Guid? BarberShopId,
    string? ServiceName,
    int Page = 1,
    int PageSize = 20) : IRequest<List<BarberDto>>;

// Service Queries
public record GetServiceByIdQuery(Guid Id) : IRequest<ServiceDto?>;

public record GetServicesByShopIdQuery(
    Guid BarberShopId,
    int Page = 1,
    int PageSize = 20) : IRequest<List<ServiceDto>>;

public record GetServicesQuery(
    Guid? BarberShopId,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? MinDurationMinutes,
    int? MaxDurationMinutes,
    int Page = 1,
    int PageSize = 20) : IRequest<List<ServiceDto>>;
