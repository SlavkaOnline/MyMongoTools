using MongoDB.Driver;

namespace PollingDomainEvents;

public static class DomainEventPublisher<TCollection, TEvent>
where TCollection : IHasDomainEvents<TEvent>
where TEvent : IDomainEvent
{
    public static UpdateDefinition<TCollection> PublishEvent(UpdateDefinition<TCollection> updateDefinition, TEvent @event)
    {
        return updateDefinition
            .Inc(x => x.DomainEventsCount, 1)
            .Push(x => x.DomainEvents, @event);
    }
    
    public static UpdateDefinition<TCollection> PublishEvents(UpdateDefinition<TCollection> updateDefinition, IReadOnlyCollection<TEvent> @events)
    {
        return updateDefinition
            .Inc(x => x.DomainEventsCount, events.Count)
            .PushEach(x => x.DomainEvents, @events);
    }
}