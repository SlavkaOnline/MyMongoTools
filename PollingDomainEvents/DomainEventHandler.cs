using Elastic.Apm;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace PollingDomainEvents;

/// <summary>
/// Обработчик доменных событий из коллекции.
/// Обрабатывает параллельно одно событие за раз в сущностях, несколькими конкретными обработчиками <see cref="IEventHandler"/>
/// В случае исключения, событие считается не обработанным, необходимо следить за этим в <see cref="IEventHandler"/>
/// </summary>
public abstract class DomainEventHandler<TCollection, TId, TEvent, TEventHandler>(
    IDistributedLockService distributedLockService,
    IMongoProvider mongoProvider,
    IEnumerable<TEventHandler> eventHandlers,
    ILogger<DomainEventHandler<TCollection, TId, TEvent, TEventHandler>> logger)
    where TCollection : IHasDomainEventsWithId<TId, TEvent>
    where TEventHandler : IEventHandler
    where TEvent : IDomainEvent
{
    private readonly IMongoCollection<TCollection> _collection = mongoProvider.GetCollection<TCollection>();

    public async Task Handle(CancellationToken cancellationToken)
    {
        const int batchSize = 100;

        var project = Builders<TCollection>.Projection
            .Include(x => x.Id)
            .Include(x => x.DomainEventsCount)
            .Slice(x => x.DomainEvents, 0, 1);
        
        using var cursor = await _collection
            .Find(Builders<TCollection>.Filter.Gt(x => x.DomainEventsCount, 0), new FindOptions()
            {
                BatchSize = batchSize,
            })
            .Project<EntityWithEventProjection<TId, TEvent>>(project)
            .ToCursorAsync(cancellationToken);

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = cancellationToken };

        while (await cursor.MoveNextAsync(cancellationToken))
        {
            await Parallel.ForEachAsync(cursor.Current, parallelOptions, async (item, ct) =>
            {
                await using var redlock = await distributedLockService.LockAsync($"domain_events_{typeof(TCollection).Name}_{item.Id}", TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), cancellationToken);

                if (!redlock.IsAcquired)
                    return;

                var entity = await _collection
                    .Find(
                        Builders<TCollection>.Filter.Eq(x => x.Id, item.Id)
                        & Builders<TCollection>.Filter.Gt(x => x.DomainEventsCount, 0)
                    )
                    .Project<EntityWithEventProjection<TId, TEvent>>(project)
                    .FirstOrDefaultAsync(ct);

                if (entity is null)
                    return;

                var @event = entity.DomainEvents.SingleOrDefault();

                if (@event is null)
                    return;

                var transaction = Agent.Config.Enabled
                    ? Agent.Tracer.StartTransaction($"{@event.GetType().Name}", "DomainEvents")
                    : null;
                
                transaction?.SetLabel("event_id", @event.EventId);

                var handlers = eventHandlers.ToArray();

                if (handlers.Length == 0)
                {
                    logger.LogError("Не настроен ни один обработчик {type}", typeof(TEventHandler).Name);
                    return;
                }
                
                try
                {
                    foreach (var handler in eventHandlers)
                    {
                        await handler.Handle(@event, cancellationToken);
                    }

                    var update = Builders<TCollection>.Update
                        .Inc(x => x.DomainEventsCount, -1)
                        .PopFirst(x => x.DomainEvents);

                    await _collection.UpdateOneAsync(
                        Builders<TCollection>.Filter.Eq(x => x.Id, item.Id),
                        update,
                        new UpdateOptions {IsUpsert = false},
                        ct
                    );
                }
                catch (Exception ex)
                {
                    transaction?.CaptureException(ex);
                }
                finally
                {
                    transaction?.End();
                }
            });
        }
    }
}