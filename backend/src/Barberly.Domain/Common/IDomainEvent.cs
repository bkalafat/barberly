namespace Barberly.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that has happened in the domain that domain experts care about.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
