using MediatR;

namespace Barberly.Application.Models;

// BarberShop DTOs
public record BarberShopDto(
    Guid Id,
    string Name,
    string Description,
    AddressDto Address,
    string Phone,
    string Email,
    string? Website,
    bool IsActive,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    DayOfWeek[] WorkingDays,
    decimal Latitude,
    decimal Longitude,
    decimal AverageRating,
    int TotalReviews,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record AddressDto(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country);

// Barber DTOs
public record BarberDto(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    string? Bio,
    string? ProfileImageUrl,
    bool IsActive,
    int YearsOfExperience,
    decimal AverageRating,
    int TotalReviews,
    Guid BarberShopId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// Service DTOs
public record ServiceDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int DurationInMinutes,
    bool IsActive,
    string? ImageUrl,
    Guid BarberShopId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// Request models
public record CreateBarberShopRequest(
    string Name,
    string Description,
    CreateAddressRequest Address,
    string Phone,
    string Email,
    string? Website,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    DayOfWeek[] WorkingDays,
    decimal Latitude,
    decimal Longitude);

public record CreateAddressRequest(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country);

public record UpdateBarberShopRequest(
    string Name,
    string Description,
    string Phone,
    string Email,
    string? Website);

public record CreateBarberRequest(
    string FullName,
    string Email,
    string Phone,
    Guid BarberShopId,
    int YearsOfExperience,
    string? Bio);

public record UpdateBarberRequest(
    string FullName,
    string Email,
    string Phone,
    string? Bio);

public record CreateServiceRequest(
    string Name,
    string Description,
    decimal Price,
    int DurationInMinutes,
    Guid BarberShopId);

public record UpdateServiceRequest(
    string Name,
    string Description,
    decimal Price,
    int DurationInMinutes);

// Query parameters
public record GetBarberShopsQuery(
    decimal? Latitude,
    decimal? Longitude,
    double? RadiusKm,
    string? ServiceName,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20) : IRequest<List<BarberShopDto>>;

public record GetBarbersQuery(
    Guid? BarberShopId,
    string? ServiceName,
    int Page = 1,
    int PageSize = 20) : IRequest<List<BarberDto>>;

public record GetServicesQuery(
    Guid? BarberShopId,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? MinDurationMinutes,
    int? MaxDurationMinutes,
    int Page = 1,
    int PageSize = 20) : IRequest<List<ServiceDto>>;
