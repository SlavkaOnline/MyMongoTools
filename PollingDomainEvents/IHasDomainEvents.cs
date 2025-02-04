namespace PollingDomainEvents;

public interface IHasId<TId>
{
     TId Id { get; }
}


/// <summary>
/// Интерфейс для добавления к колекции чтобы определить доменные события
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IHasDomainEvents<TEvent>
{
    int DomainEventsCount { get; init; }
    
    IReadOnlyCollection<TEvent> DomainEvents { get; init; }
}

/// <summary>
/// Интерфейс для добавления к колекции чтобы определить доменные события c Id
/// </summary>
/// <typeparam name="TEvent"></typeparam>
/// <typeparam name="TId"></typeparam>
public interface IHasDomainEventsWithId<TId, TEvent> : IHasId<TId>, IHasDomainEvents<TEvent>
{
    static abstract string Collection { get; }
}