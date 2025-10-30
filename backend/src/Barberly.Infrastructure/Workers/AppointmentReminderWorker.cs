using Barberly.Application.Interfaces;
using Barberly.Application.Models;
using Barberly.Application.Notifications.Interfaces;
using Barberly.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Barberly.Infrastructure.Workers;

/// <summary>
/// Background worker that sends appointment reminders 24 hours before the appointment time.
/// </summary>
public sealed class AppointmentReminderWorker : BackgroundService
{
    private readonly ILogger<AppointmentReminderWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly int _reminderHours;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

    public AppointmentReminderWorker(
        ILogger<AppointmentReminderWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        
        _reminderHours = int.Parse(
            configuration["NotificationSettings:ReminderHours"] ?? "24");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Appointment Reminder Worker started. Reminder hours: {Hours}, Check interval: {Interval}",
            _reminderHours,
            _checkInterval);

        // Wait a bit before starting to allow the system to initialize
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAppointmentRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing appointment reminders");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Appointment Reminder Worker stopped");
    }

    private async Task ProcessAppointmentRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var barberRepository = scope.ServiceProvider.GetRequiredService<IBarberRepository>();
        var serviceRepository = scope.ServiceProvider.GetRequiredService<IServiceRepository>();
        var shopRepository = scope.ServiceProvider.GetRequiredService<IBarberShopRepository>();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<INotificationOutboxRepository>();
        var templateService = scope.ServiceProvider.GetRequiredService<IEmailTemplateService>();

        // Calculate the time window for reminders
        var now = DateTimeOffset.UtcNow;
        var reminderStart = now.AddHours(_reminderHours);
        var reminderEnd = reminderStart.AddHours(1); // 1-hour window

        _logger.LogDebug(
            "Checking for appointments between {Start} and {End}",
            reminderStart,
            reminderEnd);

        // Get all appointments in the reminder window that aren't cancelled
        var appointments = await GetAppointmentsInTimeRangeAsync(
            appointmentRepository,
            reminderStart,
            reminderEnd,
            cancellationToken);

        if (appointments.Count == 0)
        {
            _logger.LogDebug("No appointments found in the reminder window");
            return;
        }

        _logger.LogInformation("Found {Count} appointments needing reminders", appointments.Count);

        foreach (var appointment in appointments)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                // Check if we've already sent a reminder for this appointment
                var existingReminders = await outboxRepository.GetPendingAsync(100, cancellationToken);
                var alreadySent = existingReminders.Any(n =>
                    n.EventType == "AppointmentReminder" &&
                    n.Metadata.Contains(appointment.Id.ToString()));

                if (alreadySent)
                {
                    _logger.LogDebug("Reminder already sent for appointment {Id}", appointment.Id);
                    continue;
                }

                await CreateReminderNotificationAsync(
                    appointment,
                    userRepository,
                    barberRepository,
                    serviceRepository,
                    shopRepository,
                    outboxRepository,
                    templateService,
                    cancellationToken);

                _logger.LogInformation(
                    "Created reminder notification for appointment {Id}",
                    appointment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating reminder for appointment {Id}",
                    appointment.Id);
            }
        }

        _logger.LogInformation("Finished processing appointment reminders");
    }

    private Task<List<Appointment>> GetAppointmentsInTimeRangeAsync(
        IAppointmentRepository appointmentRepository,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        // Note: This is a simplified implementation.
        // In a real scenario, you'd want a repository method to query appointments by time range.
        // For now, we'll need to add this method to IAppointmentRepository.
        // This is a placeholder that will need to be implemented properly.
        
        // TODO: Add GetByTimeRangeAsync to IAppointmentRepository
        return Task.FromResult(new List<Appointment>());
    }

    private async Task CreateReminderNotificationAsync(
        Appointment appointment,
        IUserRepository userRepository,
        IBarberRepository barberRepository,
        IServiceRepository serviceRepository,
        IBarberShopRepository shopRepository,
        INotificationOutboxRepository outboxRepository,
        IEmailTemplateService templateService,
        CancellationToken cancellationToken)
    {
        // Fetch related entities
        var user = await userRepository.GetByIdAsync(appointment.UserId, cancellationToken);
        if (user == null) return;

        var barber = await barberRepository.GetByIdAsync(appointment.BarberId, cancellationToken);
        if (barber == null) return;

        var service = await serviceRepository.GetByIdAsync(appointment.ServiceId, cancellationToken);
        if (service == null) return;

        var shop = await shopRepository.GetByIdAsync(barber.BarberShopId, cancellationToken);
        if (shop == null) return;

        // Map to DTOs
        var userDto = new UserDto(user.Id, user.Email, user.FullName, user.Role);
        var appointmentDto = new AppointmentDto(
            appointment.Id,
            appointment.UserId,
            appointment.BarberId,
            appointment.ServiceId,
            appointment.Start,
            appointment.End,
            appointment.IsCancelled,
            appointment.CancelledAtUtc,
            appointment.CreatedAtUtc);

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

        // Render email template
        var body = await templateService.RenderAppointmentReminderAsync(
            appointmentDto, userDto, barberDto, serviceDto, shopDto);

        // Create notification
        var notification = NotificationOutbox.Create(
            "AppointmentReminder",
            user.Email,
            user.FullName,
            "Yarın Randevunuz Var - Barberly",
            body,
            JsonSerializer.Serialize(new { appointmentDto.Id, appointment.Start, appointment.End }));

        await outboxRepository.AddAsync(notification, cancellationToken);
    }
}
