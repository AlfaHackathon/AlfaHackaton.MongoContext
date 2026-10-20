using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AlfaHackaton.MongoContext;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// Создаёт все зарегистрированные индексы (AddMongoTtlIndex/AddMongoUniqueIndex/AddMongoIndex).
    /// Вызывать один раз при старте после build — по аналогии с UseMigrations():
    /// app.Services.UseMongoIndexes();
    /// </summary>
    public static async Task UseMongoIndexesAsync(this IServiceProvider provider)
    {
        var context = provider.GetRequiredService<IMongoContext>();
        var logger = provider.GetService<ILoggerFactory>()?.CreateLogger("AlfaHackaton.MongoContext");

        foreach (var definition in provider.GetServices<MongoIndexDefinition>())
        {
            await definition.CreateAsync(context);
            logger?.LogInformation("Mongo index ensured: {Collection}", definition.CollectionName);
        }
    }
}
