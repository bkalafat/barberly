using Barberly.Application.Interfaces;
using Barberly.Application.Notifications.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Barberly.Infrastructure.Workers;

/// <summary>
/// Background worker that processes pending email notifications from the outbox.
/// Implements the Transactional Outbox pattern for reliable message delivery.
/// </summary>
public sealed class EmailNotificationWorker : BackgroundService
{
    private readonly ILogger<EmailNotificationWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly int _processingIntervalSeconds;
    private readonly int _batchSize;

    public EmailNotificationWorker(
        ILogger<EmailNotificationWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        
        _processingIntervalSeconds = int.Parse(
            configuration["NotificationSettings:ProcessingIntervalSeconds"] ?? "30");
        _batchSize = int.Parse(
            configuration["NotificationSettings:BatchSize"] ?? "10");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Email Notification Worker started. Processing interval: {Interval}s, Batch size: {BatchSize}",
            _processingIntervalSeconds,
            _batchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing notifications");
            }

            await Task.Delay(TimeSpan.FromSeconds(_processingIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Email Notification Worker stopped");
    }

    private async Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<INotificationOutboxRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var pendingNotifications = await outboxRepository.GetPendingAsync(_batchSize, cancellationToken);

        if (pendingNotifications.Count == 0)
        {
            _logger.LogDebug("No pending notifications to process");
            return;
        }

        _logger.LogInformation("Processing {Count} pending notifications", pendingNotifications.Count);

        foreach (var notification in pendingNotifications)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                // Mark as processing
                notification.MarkAsProcessing();
                await outboxRepository.UpdateAsync(notification, cancellationToken);

                // Send email
                var success = await emailService.SendEmailAsync(
                    notification.RecipientEmail,
                    notification.Subject,
                    notification.Body,
                    cancellationToken);

                if (success)
                {
                    notification.MarkAsSent();
                    _logger.LogInformation(
                        "Email sent successfully to {Email} - Event: {EventType}",
                        notification.RecipientEmail,
                        notification.EventType);
                }
                else
                {
                    notification.MarkAsFailed("Email sending failed (SMTP error or timeout)");
                    _logger.LogWarning(
                        "Failed to send email to {Email} - Event: {EventType}, Retry count: {RetryCount}",
                        notification.RecipientEmail,
                        notification.EventType,
                        notification.RetryCount);
                }

                await outboxRepository.UpdateAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing notification {Id} to {Email}",
                    notification.Id,
                    notification.RecipientEmail);

                try
                {
                    notification.MarkAsFailed($"Exception: {ex.Message}");
                    await outboxRepository.UpdateAsync(notification, cancellationToken);
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(
                        updateEx,
                        "Failed to update notification {Id} after error",
                        notification.Id);
                }
            }
        }

        _logger.LogInformation("Finished processing batch of {Count} notifications", pendingNotifications.Count);
    }
}
