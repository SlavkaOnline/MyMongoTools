using System.Reflection;
using MongoDB.Bson.Serialization;

namespace ClassMapping.Tests;

public class MongoCollectionClassMappingTests : IAsyncLifetime
{
    public class Model
    {
        public Guid ModelId { get; set; }
        
        public string Name { get; set; } = null!;
        private class ModelConfigurator : IMongoCollectionClassMapping<Model>
        {
            public void ClassMap(BsonClassMap<Model> cm)
            {
                cm.MapIdProperty(x => x.ModelId);
                cm.MapProperty(x => x.Name).SetIgnoreIfNull(true);
                cm.AutoMap();
            }
        }
    }
    
    public class AnotherModel
    {
        private class AnotherModelConfigurator : IMongoCollectionClassMapping<AnotherModel>
        {
            public void ClassMap(BsonClassMap<AnotherModel> cm)
            {
                cm.AutoMap();
            }
        }
    }

    public class SimpleModel
    {
        
    }
    
    [Fact]
    public void NoClassMapped()
    {
        Assert.False(BsonClassMap.IsClassMapRegistered(typeof(Model)));
        Assert.False(BsonClassMap.IsClassMapRegistered(typeof(AnotherModel)));
    }
    
    [Fact]
    public void ClassMapOnlyWithMapper()
    {
        MongoCollectionClassMapping.ApplyClassMappingFromAssembly(typeof(Model).Assembly);

        Assert.False(BsonClassMap.IsClassMapRegistered(typeof(SimpleModel)));
    }
    
    [Fact]
    public void ClassMap()
    {
        MongoCollectionClassMapping.ApplyClassMappingFromAssembly(typeof(Model).Assembly);
        
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(Model)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(AnotherModel)));
    }
    
    [Fact]
    public void ClassMapSeveralTimes()
    {
        MongoCollectionClassMapping.ApplyClassMappingFromAssembly(typeof(Model).Assembly);
        MongoCollectionClassMapping.ApplyClassMappingFromAssembly(typeof(Model).Assembly);
        
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(Model)));
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(AnotherModel)));
    }
    
    [Fact]
    public void ClassMapAlreadyMapped()
    {
        BsonClassMap.RegisterClassMap<Model>();
        
        MongoCollectionClassMapping.ApplyClassMappingFromAssembly(typeof(Model).Assembly);
        
        Assert.True(BsonClassMap.IsClassMapRegistered(typeof(Model)));
    }

    [Fact]
    public void ClassMapPropertyValid()
    {
        MongoCollectionClassMapping.ApplyClassMappingFromAssembly(typeof(Model).Assembly);

        var maps = BsonClassMapHelper.GetClassMaps();
        
        var classMap = maps[typeof(Model)];
        
        Assert.Equal(nameof(Model.ModelId), classMap.IdMemberMap.MemberName);
        
        Assert.True(classMap.DeclaredMemberMaps.Single(x => x.MemberName == nameof(Model.Name)).IgnoreIfNull);
    }
    
    public Task InitializeAsync()
    {
        BsonClassMapHelper.Clear();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private static class BsonClassMapHelper
    {
        public static Dictionary<Type, BsonClassMap> GetClassMaps()
        {
            var registeredClassMaps = BsonClassMap.GetRegisteredClassMaps();
            
            if (!registeredClassMaps.Any())
                return new Dictionary<Type, BsonClassMap>();
            
            var classMap = BsonClassMap.GetRegisteredClassMaps().First();
            
            var fieldInfo = typeof(BsonClassMap).GetField("__classMaps", BindingFlags.Static | BindingFlags.NonPublic)!;
            var classMaps = fieldInfo.GetValue(classMap) as Dictionary<Type, BsonClassMap>;
            
            return classMaps!;
        }

        public static void Clear()
        {
            var map = GetClassMaps();
            if (map.Count != 0)    
                map.Clear();
        }
    }
}