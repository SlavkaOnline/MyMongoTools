namespace PollingDomainEvents;

public record EntityWithEventProjection<TId, TEvent>(
    TId Id,
    int DomainEventsCount,
    IReadOnlyCollection<TEvent> DomainEvents
    ) : IHasId<TId>, IHasDomainEvents<TEvent>
{
}