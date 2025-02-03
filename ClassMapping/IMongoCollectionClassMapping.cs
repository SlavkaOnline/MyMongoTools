using MongoDB.Bson.Serialization;

namespace Plaid.Common.Infrastructure.Mongo.Configurations;

/// <summary>
/// Интерфейс для маппинга класса
/// аналог https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.ientitytypeconfiguration-1?view=efcore-8.0
/// </summary>
/// <typeparam name="TClass">Класс коллекции</typeparam>
public interface IMongoCollectionClassMapping<TClass>
    where TClass : class
{
    void ClassMap(BsonClassMap<TClass> cm);
}