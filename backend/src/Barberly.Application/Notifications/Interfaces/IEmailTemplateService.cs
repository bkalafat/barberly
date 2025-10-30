using Barberly.Application.Models;

namespace Barberly.Application.Notifications.Interfaces;

/// <summary>
/// Service for rendering email templates.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Renders an appointment confirmation email.
    /// </summary>
    Task<string> RenderAppointmentConfirmationAsync(
        AppointmentDto appointment,
        UserDto user,
        BarberDto barber,
        ServiceDto service,
        BarberShopDto shop);

    /// <summary>
    /// Renders an appointment reminder email.
    /// </summary>
    Task<string> RenderAppointmentReminderAsync(
        AppointmentDto appointment,
        UserDto user,
        BarberDto barber,
        ServiceDto service,
        BarberShopDto shop);

    /// <summary>
    /// Renders an appointment cancellation email.
    /// </summary>
    Task<string> RenderAppointmentCancellationAsync(
        AppointmentDto appointment,
        UserDto user,
        BarberDto barber,
        ServiceDto service,
        BarberShopDto shop,
        DateTimeOffset cancelledAt);
}
