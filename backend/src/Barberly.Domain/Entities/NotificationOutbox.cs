using Barberly.Domain.Common;

namespace Barberly.Domain.Entities;

/// <summary>
/// Represents an outbox entry for reliable notification delivery.
/// Follows the Transactional Outbox pattern for eventual consistency.
/// </summary>
public sealed class NotificationOutbox : Entity
{
    public string EventType { get; private set; } = string.Empty;
    public string RecipientEmail { get; private set; } = string.Empty;
    public string RecipientName { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string Metadata { get; private set; } = string.Empty;
    public NotificationStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public DateTimeOffset ProcessedAtUtc { get; private set; }
    public string? ErrorMessage { get; private set; }

    private NotificationOutbox() { }

    private NotificationOutbox(
        string eventType,
        string recipientEmail,
        string recipientName,
        string subject,
        string body,
        string metadata,
        int maxRetries = 3)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type cannot be empty", nameof(eventType));
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException("Recipient email cannot be empty", nameof(recipientEmail));
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be empty", nameof(subject));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be empty", nameof(body));
        if (maxRetries < 0)
            throw new ArgumentException("Max retries cannot be negative", nameof(maxRetries));

        EventType = eventType;
        RecipientEmail = recipientEmail;
        RecipientName = recipientName;
        Subject = subject;
        Body = body;
        Metadata = metadata;
        Status = NotificationStatus.Pending;
        RetryCount = 0;
        MaxRetries = maxRetries;
    }

    public static NotificationOutbox Create(
        string eventType,
        string recipientEmail,
        string recipientName,
        string subject,
        string body,
        string metadata = "{}",
        int maxRetries = 3)
    {
        return new NotificationOutbox(
            eventType,
            recipientEmail,
            recipientName,
            subject,
            body,
            metadata,
            maxRetries);
    }

    public void MarkAsProcessing()
    {
        if (Status != NotificationStatus.Pending && Status != NotificationStatus.Failed)
            throw new InvalidOperationException($"Cannot mark notification as processing when status is {Status}");

        Status = NotificationStatus.Processing;
        MarkAsUpdated();
    }

    public void MarkAsSent()
    {
        if (Status != NotificationStatus.Processing)
            throw new InvalidOperationException($"Cannot mark notification as sent when status is {Status}");

        Status = NotificationStatus.Sent;
        ProcessedAtUtc = DateTimeOffset.UtcNow;
        ErrorMessage = null;
        MarkAsUpdated();
    }

    public void MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));

        RetryCount++;
        ErrorMessage = errorMessage;

        if (RetryCount >= MaxRetries)
        {
            Status = NotificationStatus.Failed;
            ProcessedAtUtc = DateTimeOffset.UtcNow;
        }
        else
        {
            Status = NotificationStatus.Pending; // Ready for retry
        }

        MarkAsUpdated();
    }

    public bool CanRetry()
    {
        return Status == NotificationStatus.Pending && RetryCount < MaxRetries;
    }
}

/// <summary>
/// Status of a notification in the outbox
/// </summary>
public enum NotificationStatus
{
    Pending = 0,
    Processing = 1,
    Sent = 2,
    Failed = 3
}
