namespace AlfaHackaton.MongoContext;

/// <summary>
/// Настройки подключения к MongoDB (аналог DapperSettings из AlfaHackaton.DapperContext).
/// </summary>
public sealed record MongoSettings(string ConnectionString, string DatabaseName);
