namespace PollingDomainEvents;

public interface IDomainEvent
{
     string EventId { get; }
     
     DateTime Created { get; init; } 
}