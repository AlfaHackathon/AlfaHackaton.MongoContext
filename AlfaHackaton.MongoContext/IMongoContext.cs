using MongoDB.Driver;

namespace AlfaHackaton.MongoContext;

/// <summary>
/// Доступ к Mongo-базе: коллекции по имени, настройки для тестов.
/// </summary>
public interface IMongoContext
{
    /// <summary>База данных, указанная в MongoSettings.</summary>
    IMongoDatabase Database { get; }

    /// <summary>Коллекция с указанным именем из этой базы.</summary>
    IMongoCollection<TDocument> GetCollection<TDocument>(string name);
}
