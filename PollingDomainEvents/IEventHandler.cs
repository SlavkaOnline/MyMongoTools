namespace PollingDomainEvents;

/// <summary>
/// Обработчик событий
/// </summary>
public interface IEventHandler
{
    /// <summary>
    /// Обработка события
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task Handle(object @event, CancellationToken cancellation);
}

/// <summary>
/// Типизированный обработчик событий
/// </summary>
/// <typeparam name="TEvent">Тип События</typeparam>
public interface IEventHandler<TEvent> : IEventHandler
    where TEvent : IDomainEvent
{
    /// <inheritdoc/>
    Task IEventHandler.Handle(object @event, CancellationToken cancellation)
    {
        return @event.GetType() != typeof(TEvent) 
            ? Task.CompletedTask 
            : Handle((TEvent)@event, cancellation);
    }

    /// <summary>
    /// Обработка события
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task Handle(TEvent @event, CancellationToken cancellation);
}