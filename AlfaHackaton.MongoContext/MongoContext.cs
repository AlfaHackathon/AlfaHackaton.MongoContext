using MongoDB.Driver;

namespace AlfaHackaton.MongoContext;

/// <inheritdoc cref="IMongoContext" />
public sealed class MongoContext(MongoSettings settings) : IMongoContext
{
    private readonly MongoClient _client = new(settings.ConnectionString);

    public IMongoDatabase Database => _client.GetDatabase(settings.DatabaseName);

    public IMongoCollection<TDocument> GetCollection<TDocument>(string name) =>
        Database.GetCollection<TDocument>(name);
}
