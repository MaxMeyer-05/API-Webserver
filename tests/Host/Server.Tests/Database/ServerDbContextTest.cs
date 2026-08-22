using Microsoft.EntityFrameworkCore;

using Server.Database.DbContexts;
using Server.Tests.TestData;

namespace Server.Tests.Database;

[Trait("Category", "Database")]
[Trait("Module", "Server")]
public class ServerDbContextTest : IDisposable
{
    private readonly ServerDbContext _context;
    private readonly IDisposable _connection;

    public ServerDbContextTest()
    {
        var (context, connection) = ServerTestData.CreateInMemoryServerDbContext();
        _context = context;
        _connection = connection;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region User CRUD Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CanInsertAndRetrieveUserAsync()
    {
        // Arrange
        var user = ServerTestData.CreateTestUser(email: "john.doe@example.com", firstName: "John", lastName: "Doe");

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Users.FirstOrDefaultAsync(u => u.Email == "john.doe@example.com");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(user.Id, retrieved.Id);
        Assert.Equal("John", retrieved.FirstName);
        Assert.Equal("Doe", retrieved.LastName);
        Assert.Equal("user", retrieved.Role);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task CanUpdateUserRoleAndNameAsync()
    {
        // Arrange
        var user = ServerTestData.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        user.Role = "admin";
        user.FirstName = "Alexander";
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var updated = await _context.Users.FindAsync(user.Id);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("admin", updated.Role);
        Assert.Equal("Alexander", updated.FirstName);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task CanDeleteUserAsync()
    {
        // Arrange
        var user = ServerTestData.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        var exists = await _context.Users.AnyAsync(u => u.Id == user.Id);

        // Assert
        Assert.False(exists);
    }

    #endregion

    #region Entity Default Constraints Tests

    [Fact]
    [Trait("Action", "Validation")]
    public void User_ShouldHaveDefaultValuesAssigned()
    {
        // Act
        var user = ServerTestData.CreateTestUser();

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("user", user.Role);
    }

    #endregion
}