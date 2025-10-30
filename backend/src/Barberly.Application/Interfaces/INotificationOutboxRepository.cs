using Barberly.Domain.Entities;

namespace Barberly.Application.Interfaces;

/// <summary>
/// Repository for notification outbox entries.
/// </summary>
public interface INotificationOutboxRepository
{
    /// <summary>
    /// Adds a notification to the outbox.
    /// </summary>
    Task AddAsync(NotificationOutbox notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a notification by its ID.
    /// </summary>
    Task<NotificationOutbox?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending notifications ready for processing.
    /// </summary>
    /// <param name="batchSize">Maximum number of notifications to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<List<NotificationOutbox>> GetPendingAsync(
        int batchSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a notification in the outbox.
    /// </summary>
    Task UpdateAsync(NotificationOutbox notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of pending notifications.
    /// </summary>
    Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets failed notifications for monitoring.
    /// </summary>
    Task<List<NotificationOutbox>> GetFailedAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);
}
