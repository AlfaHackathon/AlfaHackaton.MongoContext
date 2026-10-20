using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace AlfaHackaton.MongoContext;

/// <summary>
/// Описание индекса, который нужно создать при старте сервиса.
/// Создание идемпотентно: createIndex с той же спецификацией ничего не ломает.
/// </summary>
public sealed record MongoIndexDefinition(
    string CollectionName,
    Func<IMongoContext, Task> CreateAsync);

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует IMongoContext (singleton) по настройкам.
    /// В Program: services.AddMongo(new MongoSettings(connectionString, databaseName));
    /// </summary>
    public static IServiceCollection AddMongo(this IServiceCollection services, MongoSettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton<IMongoContext, MongoContext>();
        return services;
    }

    /// <summary>
    /// TTL-индекс по полю-дате: документы удаляются Mongo автоматически, когда
    /// значение поля + expireAfter остаётся позади (проверка ~раз в минуту).
    /// expireAfter = TimeSpan.Zero — удалять ровно в момент из поля («протухающие» документы).
    /// </summary>
    public static IServiceCollection AddMongoTtlIndex<TDocument>(
        this IServiceCollection services,
        string collectionName,
        Expression<Func<TDocument, object?>> field,
        TimeSpan expireAfter) =>
        services.AddRawIndex(collectionName, field, new CreateIndexOptions { ExpireAfter = expireAfter });

    /// <summary>Уникальный индекс по полю (например, короткий код инвайта).</summary>
    public static IServiceCollection AddMongoUniqueIndex<TDocument>(
        this IServiceCollection services,
        string collectionName,
        Expression<Func<TDocument, object?>> field) =>
        services.AddRawIndex(collectionName, field, new CreateIndexOptions { Unique = true });

    /// <summary>Обычный (неуникальный, без TTL) индекс по полю.</summary>
    public static IServiceCollection AddMongoIndex<TDocument>(
        this IServiceCollection services,
        string collectionName,
        Expression<Func<TDocument, object?>> field) =>
        services.AddRawIndex(collectionName, field, new CreateIndexOptions());

    private static IServiceCollection AddRawIndex(
        this IServiceCollection services,
        string collectionName,
        LambdaExpression field,
        CreateIndexOptions options)
    {
        services.AddSingleton(new MongoIndexDefinition(collectionName, async context =>
        {
            var collection = context.Database.GetCollection<BsonDocument>(collectionName);
            var keys = new BsonDocument { { GetFieldName(field), 1 } };
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(keys, options));
        }));
        return services;
    }

    /// <summary>Имя поля BSON из выражения x => x.Field (учитывает BsonElementAttribute).</summary>
    private static string GetFieldName(LambdaExpression expression)
    {
        var body = expression.Body is UnaryExpression { Operand: MemberExpression member }
            ? member
            : expression.Body as MemberExpression;
        var memberInfo = body?.Member
            ?? throw new ArgumentException("Ожидается выражение вида x => x.Field", nameof(expression));

        var bsonElement = memberInfo.GetCustomAttribute<BsonElementAttribute>();
        return bsonElement?.ElementName ?? memberInfo.Name;
    }
}
