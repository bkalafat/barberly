namespace Barberly.Application.Interfaces;

public interface IEmailService
{
    Task<SendEmailResult> SendAppointmentConfirmationAsync(Guid appointmentId, CancellationToken ct = default);
    Task<SendEmailResult> SendAppointmentReminderAsync(Guid appointmentId, CancellationToken ct = default);
    Task<SendEmailResult> SendAppointmentCancellationAsync(Guid appointmentId, CancellationToken ct = default);
}

public record SendEmailResult(bool Success, string? MessageId, string? ErrorMessage);
