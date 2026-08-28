namespace SharedKernel.Modules;

/// <summary>
/// Represents information about a database, 
/// including its name, provider, and connection string key.
/// </summary>
/// <param name="Name">The name of the database.</param>
/// <param name="Provider">The database provider (e.g., "SQLite", "MySQL", "PostgreSQL").</param>
/// <param name="ConnectionStringKey">The key for the connection string in the configuration.</param>
public record DatabaseInfo(
    string Name,
    string Provider,
    string ConnectionStringKey
);