using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Server.Database.DbContexts;
using Server.Database.Entities;

namespace Server.Tests.TestData;

public static class ServerTestData
{
    #region Configuration Fixtures

    public static IConfiguration CreateConfiguration(
        string? groceryStoreDb = "Data Source=:memory:",
        string? serverDb = "Data Source=:memory:")
    {
        var settings = new Dictionary<string, string?>();

        if (groceryStoreDb is not null)
        {
            settings["ConnectionStrings:GroceryStore"] = groceryStoreDb;
        }

        if (serverDb is not null)
        {
            settings["ConnectionStrings:Server"] = serverDb;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    #endregion

    #region Entity Fixtures

    public static User CreateTestUser(
        string email = "max.mustermann@example.com",
        string firstName = "Max",
        string lastName = "Mustermann",
        string role = "user",
        string passwordHash = "hashed_pw_123") => new()
    {
        Id = Guid.NewGuid(),
        Email = email,
        FirstName = firstName,
        LastName = lastName,
        Role = role,
        PasswordHash = passwordHash,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    #endregion

    #region DbContext Fixtures

    public static (ServerDbContext Context, SqliteConnection Connection) CreateInMemoryServerDbContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ServerDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ServerDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }

    #endregion
}