using MediatR;

namespace Barberly.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that has happened in the domain that domain experts care about.
/// Extends INotification for MediatR integration.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredOn { get; }
}
