# AlfaHackaton.MongoContext

MongoDB-инфраструктура для сервисов AlfaHackathon (аналог AlfaHackaton.DapperContext, только для Mongo):
контекст, настройки и управление индексами при старте — **включая TTL-индексы для самоочистки коллекций**.

## Использование

```csharp
using AlfaHackaton.MongoContext;

// Program.cs — регистрация
builder.Services.AddMongo(new MongoSettings(
    builder.Configuration["Mongo:ConnectionString"] ?? "mongodb://localhost:27017",
    builder.Configuration["Mongo:Database"] ?? "my-service"));

// TTL: Mongo сама удаляет документ, когда ExpiresAt останется позади (проверка ~раз в минуту)
builder.Services.AddMongoTtlIndex<InviteDocument>("invites", x => x.ExpiresAt, TimeSpan.Zero);
// Уникальность короткого кода
builder.Services.AddMongoUniqueIndex<InviteDocument>("invites", x => x.Code);
// Обычный индекс для частых запросов
builder.Services.AddMongoIndex<InviteDocument>("invites", x => x.CompanyId);

var app = builder.Build();

// Создание индексов при старте (идемпотентно)
await app.Services.UseMongoIndexesAsync();
```

Сущность:

```csharp
public sealed class InviteDocument
{
    [BsonId, BsonRepresentation(BsonType.String)] public Guid Id { get; init; }
    [BsonRepresentation(BsonType.String)] public Guid CompanyId { get; init; }
    public string Code { get; init; } = null!;
    public DateTime ExpiresAt { get; init; }   // поле TTL-индекса
}
```

## Публикация в GitHub Packages

```bash
dotnet pack -c Release
dotnet nuget push bin/Release/AlfaHackaton.MongoContext.1.0.0.nupkg \
  --source https://nuget.pkg.github.com/AlfaHackathon/index.json \
  --api-key <GITHUB_TOKEN with read:packages/write:packages>
```

После публикации в сервисе-потребителе ProjectReference меняется на
`<PackageReference Include="AlfaHackaton.MongoContext" Version="[1.0.0]" />`.
