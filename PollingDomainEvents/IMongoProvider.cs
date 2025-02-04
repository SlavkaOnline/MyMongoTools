using MongoDB.Driver;

namespace PollingDomainEvents;

public interface IMongoProvider
{
    IMongoCollection<TCollection> GetCollection<TCollection>();
}