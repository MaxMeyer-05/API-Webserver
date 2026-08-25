using Moq;

using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Users;
using GroceryStore.Features.Users.Interfaces;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Users;

[Trait("Category", "Service")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Users")]
public class UserServiceTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<IUserMapper> _mapperMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _service;

    public UserServiceTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;
        _mapperMock = new Mock<IUserMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<UserService>>();

        _service = new UserService(_context, _mapperMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region CreateUserAsync Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateUserAsync_ShouldPersistUserAndReturnDto_WhenValid()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var registrationDto = UserTestData.CreateUserRegistrationDto("Max", "Mustermann", "max@example.com");
        var entityToInsert = UserTestData.CreateUser(firstName: "Max", lastName: "Mustermann", email: "max@example.com", zipCode: location.ZipCode);
        var expectedDto = UserTestData.CreateUserDto(entityToInsert.Id, "Max", "Mustermann", "max@example.com");

        _mapperMock.Setup(m => m.ToUserEntity(registrationDto)).Returns(entityToInsert);
        _mapperMock.Setup(m => m.ToUserDto(entityToInsert)).Returns(expectedDto);

        // Act
        var result = await _service.CreateUserAsync(registrationDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Max Mustermann", result.Name);
        Assert.Equal("max@example.com", result.Email);

        var persisted = await _context.Users.FindAsync(entityToInsert.Id);
        Assert.NotNull(persisted);

        _mapperMock.Verify(m => m.ToUserEntity(registrationDto), Times.Once);
        _mapperMock.Verify(m => m.ToUserDto(entityToInsert), Times.Once);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateUserAsync_ShouldThrowInvalidOperationException_WhenEmailIsAlreadyInUse()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var existingUser = GroceryStoreTestData.CreateUser(email: "duplicate@example.com", zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var registrationDto = UserTestData.CreateUserRegistrationDto(email: "duplicate@example.com");
        var entityToInsert = UserTestData.CreateUser(email: "duplicate@example.com", zipCode: location.ZipCode);

        _mapperMock.Setup(m => m.ToUserEntity(registrationDto)).Returns(entityToInsert);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateUserAsync(registrationDto));
        Assert.Equal("Email is already in use.", ex.Message);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateUserAsync_ShouldThrowInvalidOperationException_WhenPhoneNumberIsAlreadyInUse()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var existingUser = UserTestData.CreateUser(email: "user1@example.com", zipCode: location.ZipCode);
        existingUser.PhoneNumber = "0151888888";

        _context.Locations.Add(location);
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var registrationDto = UserTestData.CreateUserRegistrationDto(email: "user2@example.com");
        var entityToInsert = UserTestData.CreateUser(email: "user2@example.com", zipCode: location.ZipCode);
        entityToInsert.PhoneNumber = "0151888888";

        _mapperMock.Setup(m => m.ToUserEntity(registrationDto)).Returns(entityToInsert);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateUserAsync(registrationDto));
        Assert.Equal("Phone number is already in use.", ex.Message);
    }

    #endregion

    #region GetAllUsersAsync & GetUserByIdAsync Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllUsersAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user1 = UserTestData.CreateUser(id: Guid.NewGuid(), email: "user1@domain.de", zipCode: location.ZipCode);
        var user2 = UserTestData.CreateUser(id: Guid.NewGuid(), email: "user2@domain.de", zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.ToUserDto(It.Is<User>(u => u.Id == user1.Id)))
            .Returns(UserTestData.CreateUserDto(user1.Id, email: "user1@domain.de"));
        _mapperMock.Setup(m => m.ToUserDto(It.Is<User>(u => u.Id == user2.Id)))
            .Returns(UserTestData.CreateUserDto(user2.Id, email: "user2@domain.de"));

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, u => u.Email == "user1@domain.de");
        Assert.Contains(list, u => u.Email == "user2@domain.de");
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetUserByIdAsync_ShouldReturnMappedDto_WhenUserExists()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = UserTestData.CreateUser(zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var expectedDto = UserTestData.CreateUserDto(user.Id);
        _mapperMock.Setup(m => m.ToUserDto(It.Is<User>(u => u.Id == user.Id))).Returns(expectedDto);

        // Act
        var result = await _service.GetUserByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetUserByIdAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Act & Assert
        var missingId = Guid.NewGuid();
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetUserByIdAsync(missingId));
        Assert.Contains(missingId.ToString(), ex.Message);
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateUserAsync_ShouldCallMapperAndPersist_WhenValid()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = UserTestData.CreateUser(zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var updateDto = UserTestData.CreateUserUpdateDto(email: "updated@domain.de", phoneNumber: null);
        _mapperMock.Setup(m => m.UpdateUserEntity(user, updateDto))
            .Callback<User, UserUpdateDto>((u, dto) => u.Email = dto.Email!);

        // Act
        await _service.UpdateUserAsync(user.Id, updateDto);

        // Assert
        var updated = await _context.Users.FindAsync(user.Id);
        Assert.NotNull(updated);
        Assert.Equal("updated@domain.de", updated.Email);
        _mapperMock.Verify(m => m.UpdateUserEntity(user, updateDto), Times.Once);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateUserAsync_ShouldThrowInvalidOperationException_WhenNewEmailIsTakenByAnotherUser()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user1 = UserTestData.CreateUser(id: Guid.NewGuid(), email: "user1@domain.de", zipCode: location.ZipCode);
        var user2 = UserTestData.CreateUser(id: Guid.NewGuid(), email: "user2@domain.de", zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var updateDto = UserTestData.CreateUserUpdateDto(email: "user2@domain.de");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateUserAsync(user1.Id, updateDto));
        Assert.Equal("Email is already in use.", ex.Message);
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteUserAsync_ShouldAnonymizeUser_WhenPasswordMatches()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        const string rawPassword = "CorrectPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var user = UserTestData.CreateUser(passwordHash: passwordHash, zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.AnonymizeUserEntity(user))
            .Callback<User>(u =>
            {
                u.FirstName = "null";
                u.LastName = "null";
                u.Role = "anonymized_user";
            });

        // Act
        await _service.DeleteUserAsync(user.Id, rawPassword);

        // Assert
        _mapperMock.Verify(m => m.AnonymizeUserEntity(user), Times.Once);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteUserAsync_ShouldThrowInvalidOperationException_WhenPasswordIsIncorrect()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = UserTestData.CreateUser(
            passwordHash: BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
            zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteUserAsync(user.Id, "WrongPassword"));
        Assert.Equal("Invalid password.", ex.Message);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteUserAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.DeleteUserAsync(Guid.NewGuid(), "AnyPassword"));
    }

    #endregion

    #region IsEmailInUseAsync & IsPhoneNumberInUseAsync Tests

    [Fact]
    [Trait("Action", "Validation")]
    public async Task IsEmailInUseAsync_ShouldReturnTrue_WhenEmailExistsAndNotExcluded()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = UserTestData.CreateUser(email: "exist@domain.de", zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var inUse = await _service.IsEmailInUseAsync("exist@domain.de");
        var notInUseWhenExcluded = await _service.IsEmailInUseAsync("exist@domain.de", excludedUserId: user.Id);

        // Assert
        Assert.True(inUse);
        Assert.False(notInUseWhenExcluded);
    }

    [Fact]
    [Trait("Action", "Validation")]
    public async Task IsPhoneNumberInUseAsync_ShouldReturnTrue_WhenPhoneExistsAndNotExcluded()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = UserTestData.CreateUser(zipCode: location.ZipCode);
        user.PhoneNumber = "0123456789";

        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var inUse = await _service.IsPhoneNumberInUseAsync("0123456789");
        var notInUseWhenExcluded = await _service.IsPhoneNumberInUseAsync("0123456789", excludedUserId: user.Id);

        // Assert
        Assert.True(inUse);
        Assert.False(notInUseWhenExcluded);
    }

    #endregion
}