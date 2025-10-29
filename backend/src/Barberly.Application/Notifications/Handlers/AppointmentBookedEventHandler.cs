using Barberly.Application.Interfaces;
using Barberly.Application.Models;
using Barberly.Application.Notifications.Interfaces;
using Barberly.Domain.Entities;
using Barberly.Domain.Events;
using MediatR;
using System.Text.Json;

namespace Barberly.Application.Notifications.Handlers;

/// <summary>
/// Handles AppointmentBookedEvent and creates email notifications in the outbox.
/// </summary>
public sealed class AppointmentBookedEventHandler : INotificationHandler<AppointmentBookedEvent>
{
    private readonly INotificationOutboxRepository _outboxRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBarberRepository _barberRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IBarberShopRepository _shopRepository;
    private readonly IEmailTemplateService _templateService;

    public AppointmentBookedEventHandler(
        INotificationOutboxRepository outboxRepository,
        IUserRepository userRepository,
        IBarberRepository barberRepository,
        IServiceRepository serviceRepository,
        IBarberShopRepository shopRepository,
        IEmailTemplateService templateService)
    {
        _outboxRepository = outboxRepository;
        _userRepository = userRepository;
        _barberRepository = barberRepository;
        _serviceRepository = serviceRepository;
        _shopRepository = shopRepository;
        _templateService = templateService;
    }

    public async Task Handle(AppointmentBookedEvent notification, CancellationToken cancellationToken)
    {
        // Fetch related entities
        var user = await _userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user == null) return;

        var barber = await _barberRepository.GetByIdAsync(notification.BarberId, cancellationToken);
        if (barber == null) return;

        var service = await _serviceRepository.GetByIdAsync(notification.ServiceId, cancellationToken);
        if (service == null) return;

        var shop = await _shopRepository.GetByIdAsync(barber.BarberShopId, cancellationToken);
        if (shop == null) return;

        // Map to DTOs
        var userDto = new UserDto(user.Id, user.Email, user.FullName, user.Role);
        var appointmentDto = new AppointmentDto(
            notification.AppointmentId,
            notification.UserId,
            notification.BarberId,
            notification.ServiceId,
            notification.Start,
            notification.End,
            false,
            null,
            notification.OccurredOn);
        
        var barberDto = new BarberDto(
            barber.Id,
            barber.FullName,
            barber.Email,
            barber.Phone,
            barber.Bio,
            barber.ProfileImageUrl,
            barber.IsActive,
            barber.YearsOfExperience,
            barber.AverageRating,
            barber.TotalReviews,
            barber.BarberShopId,
            barber.CreatedAt,
            barber.UpdatedAt);

        var serviceDto = new ServiceDto(
            service.Id,
            service.Name,
            service.Description,
            service.Price,
            service.DurationInMinutes,
            service.IsActive,
            service.ImageUrl,
            service.BarberShopId,
            service.CreatedAt,
            service.UpdatedAt);

        var shopDto = new BarberShopDto(
            shop.Id,
            shop.Name,
            shop.Description,
            new AddressDto(
                shop.Address.Street,
                shop.Address.City,
                shop.Address.State,
                shop.Address.PostalCode,
                shop.Address.Country),
            shop.Phone,
            shop.Email,
            shop.Website,
            shop.IsActive,
            shop.OpenTime,
            shop.CloseTime,
            shop.WorkingDays,
            shop.Latitude,
            shop.Longitude,
            shop.AverageRating,
            shop.TotalReviews,
            shop.CreatedAt,
            shop.UpdatedAt);

        // Create notification for customer
        var customerBody = await _templateService.RenderAppointmentConfirmationAsync(
            appointmentDto, userDto, barberDto, serviceDto, shopDto);
        
        var customerNotification = NotificationOutbox.Create(
            "AppointmentBooked",
            user.Email,
            user.FullName,
            "Randevunuz Onaylandı - Barberly",
            customerBody,
            JsonSerializer.Serialize(new { appointmentDto.Id, notification.Start, notification.End }));

        await _outboxRepository.AddAsync(customerNotification, cancellationToken);

        // Create notification for barber
        var barberBody = await _templateService.RenderAppointmentConfirmationAsync(
            appointmentDto, userDto, barberDto, serviceDto, shopDto);

        var barberNotification = NotificationOutbox.Create(
            "AppointmentBooked",
            barber.Email,
            barber.FullName,
            "Yeni Randevu - Barberly",
            barberBody,
            JsonSerializer.Serialize(new { appointmentDto.Id, notification.Start, notification.End }));

        await _outboxRepository.AddAsync(barberNotification, cancellationToken);
    }
}
