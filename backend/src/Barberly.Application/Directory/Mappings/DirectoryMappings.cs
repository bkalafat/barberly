using Barberly.Application.Models;
using Barberly.Domain.Entities;
using Barberly.Domain.ValueObjects;

namespace Barberly.Application.Directory.Mappings;

public static class DirectoryMappings
{
    public static BarberShopDto ToDto(this BarberShop barberShop)
    {
        return new BarberShopDto(
            Id: barberShop.Id,
            Name: barberShop.Name,
            Description: barberShop.Description,
            Address: barberShop.Address.ToDto(),
            Phone: barberShop.Phone,
            Email: barberShop.Email,
            Website: barberShop.Website,
            IsActive: barberShop.IsActive,
            OpenTime: barberShop.OpenTime,
            CloseTime: barberShop.CloseTime,
            WorkingDays: barberShop.WorkingDays,
            Latitude: barberShop.Latitude,
            Longitude: barberShop.Longitude,
            AverageRating: barberShop.AverageRating,
            TotalReviews: barberShop.TotalReviews,
            CreatedAt: barberShop.CreatedAt,
            UpdatedAt: barberShop.UpdatedAt
        );
    }

    public static AddressDto ToDto(this Address address)
    {
        return new AddressDto(
            Street: address.Street,
            City: address.City,
            State: address.State,
            PostalCode: address.PostalCode,
            Country: address.Country
        );
    }

    public static BarberDto ToDto(this Barber barber)
    {
        return new BarberDto(
            Id: barber.Id,
            FullName: barber.FullName,
            Email: barber.Email,
            Phone: barber.Phone,
            Bio: barber.Bio,
            ProfileImageUrl: barber.ProfileImageUrl,
            IsActive: barber.IsActive,
            YearsOfExperience: barber.YearsOfExperience,
            AverageRating: barber.AverageRating,
            TotalReviews: barber.TotalReviews,
            BarberShopId: barber.BarberShopId,
            CreatedAt: barber.CreatedAt,
            UpdatedAt: barber.UpdatedAt
        );
    }

    public static ServiceDto ToDto(this Service service)
    {
        return new ServiceDto(
            Id: service.Id,
            Name: service.Name,
            Description: service.Description,
            Price: service.Price,
            DurationInMinutes: service.DurationInMinutes,
            IsActive: service.IsActive,
            ImageUrl: service.ImageUrl,
            BarberShopId: service.BarberShopId,
            CreatedAt: service.CreatedAt,
            UpdatedAt: service.UpdatedAt
        );
    }
}
