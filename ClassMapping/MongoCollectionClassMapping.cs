using System;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;

namespace Plaid.Common.Infrastructure.Mongo.Configurations;

public static class MongoCollectionClassMapping
{
    /// <summary>
    /// Собрать все классы которые реализуют <see cref="IMongoCollectionClassMapping<>"/>
    /// Вызвать их метод маппинга для конфигурации <see cref="BsonClassMap{TClass}"/>
    /// Вызвать <see cref="BsonClassMap.RegisterClassMap"/>
    /// </summary>
    /// <param name="assembly"></param>
    public static void ApplyClassMappingFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        
        foreach (var configuratorType in assembly.GetTypes())
        {
            if (configuratorType.GetConstructor(Type.EmptyTypes) == null)
            {
                continue;
            }

            foreach (var @interface in configuratorType.GetInterfaces())
            {
                if (!@interface.IsGenericType)
                {
                    continue;
                }

                if (@interface.GetGenericTypeDefinition() == typeof(IMongoCollectionClassMapping<>))
                {
                    var classType = @interface.GetGenericArguments()[0];
                    
                    var classMapType = typeof(BsonClassMap<>).MakeGenericType(classType);
                    var classMap = Activator.CreateInstance(classMapType);
                    
                    var configurator = Activator.CreateInstance(configuratorType);
                    var configureMethod = configuratorType
                        .GetMethods()
                        .Single(x => x.Name == nameof(IMongoCollectionClassMapping<object>.ClassMap));
                    
                    configureMethod.Invoke(configurator, new[] {classMap});
                    
                    if (!BsonClassMap.IsClassMapRegistered(classType)) 
                        BsonClassMap.RegisterClassMap((BsonClassMap)classMap);
                }
            }
        }
    }
}