using Moq;

using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Customers;
using GroceryStore.Features.Customers.Interfaces;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Customers;

[Trait("Category", "Service")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Customers")]
public class CustomerServiceTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<ICustomerMapper> _mapperMock;
    private readonly Mock<ILogger<CustomerService>> _loggerMock;
    private readonly CustomerService _service;

    public CustomerServiceTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;
        _mapperMock = new Mock<ICustomerMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<CustomerService>>();

        _service = new CustomerService(_context, _mapperMock.Object, _loggerMock.Object);
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
    public async Task CreateCustomerAsync_ShouldPersistCustomerAndReturnDto_WhenValid()
    {
        // Arrange
        var registrationDto = CustomerTestData.CreateCustomerRegistrationDto("Max", "Mustermann", "max@example.com");
        var entityToInsert = CustomerTestData.CreateCustomer(firstName: "Max", lastName: "Mustermann", email: "max@example.com", zipCode: registrationDto.ZipCode);
        var expectedDto = CustomerTestData.CreateCustomerDto(entityToInsert.Id, "Max", "Mustermann", "max@example.com");

        _mapperMock.Setup(m => m.ToCustomerEntity(registrationDto)).Returns(entityToInsert);
        _mapperMock.Setup(m => m.ToCustomerDto(entityToInsert)).Returns(expectedDto);

        // Act
        var result = await _service.CreateCustomerAsync(registrationDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Max Mustermann", result.Name);
        Assert.Equal("max@example.com", result.Email);

        var persisted = await _context.Customers.FindAsync(entityToInsert.Id);
        Assert.NotNull(persisted);

        _mapperMock.Verify(m => m.ToCustomerEntity(registrationDto), Times.Once);
        _mapperMock.Verify(m => m.ToCustomerDto(entityToInsert), Times.Once);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateCustomerAsync_ShouldThrowInvalidOperationException_WhenEmailIsAlreadyInUse()
    {
        // Arrange
        var existingUser = GroceryStoreTestData.CreateCustomer(email: "duplicate@example.com");

        _context.Customers.Add(existingUser);
        await _context.SaveChangesAsync();

        var registrationDto = CustomerTestData.CreateCustomerRegistrationDto(email: "duplicate@example.com");
        var entityToInsert = CustomerTestData.CreateCustomer(email: "duplicate@example.com");

        _mapperMock.Setup(m => m.ToCustomerEntity(registrationDto)).Returns(entityToInsert);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateCustomerAsync(registrationDto));
        Assert.Equal("Email is already in use.", ex.Message);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateCustomerAsync_ShouldThrowInvalidOperationException_WhenPhoneNumberIsAlreadyInUse()
    {
        // Arrange
        var existingUser = CustomerTestData.CreateCustomer(email: "user1@example.com");
        existingUser.PhoneNumber = "0151888888";

        _context.Customers.Add(existingUser);
        await _context.SaveChangesAsync();

        var registrationDto = CustomerTestData.CreateCustomerRegistrationDto(email: "user2@example.com");
        var entityToInsert = CustomerTestData.CreateCustomer(email: "user2@example.com");
        entityToInsert.PhoneNumber = "0151888888";

        _mapperMock.Setup(m => m.ToCustomerEntity(registrationDto)).Returns(entityToInsert);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateCustomerAsync(registrationDto));
        Assert.Equal("Phone number is already in use.", ex.Message);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateCustomerAsync_ShouldThrowInvalidOperationException_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var registrationDto = CustomerTestData.CreateCustomerRegistrationDto(
            password: "SecurePassword123!",
            confirmPassword: "DifferentPassword123!");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateCustomerAsync(registrationDto));
        Assert.Equal("Passwords do not match.", ex.Message);
        _mapperMock.Verify(m => m.ToCustomerEntity(It.IsAny<CustomerRegistrationDto>()), Times.Never);
    }

    #endregion

    #region GetAllUsersAsync & GetUserByIdAsync Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllCustomersAsync_ShouldReturnAllCustomers()
    {
        // Arrange
        var user1 = CustomerTestData.CreateCustomer(id: Guid.NewGuid(), email: "user1@domain.de");
        var user2 = CustomerTestData.CreateCustomer(id: Guid.NewGuid(), email: "user2@domain.de");

        _context.Customers.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.ToCustomerDto(It.Is<Customer>(u => u.Id == user1.Id)))
            .Returns(CustomerTestData.CreateCustomerDto(user1.Id, email: "user1@domain.de"));
        _mapperMock.Setup(m => m.ToCustomerDto(It.Is<Customer>(u => u.Id == user2.Id)))
            .Returns(CustomerTestData.CreateCustomerDto(user2.Id, email: "user2@domain.de"));

        // Act
        var result = await _service.GetAllCustomersAsync();

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, u => u.Email == "user1@domain.de");
        Assert.Contains(list, u => u.Email == "user2@domain.de");
        _mapperMock.Verify(m => m.ToCustomerDto(user1), Times.Once);
        _mapperMock.Verify(m => m.ToCustomerDto(user2), Times.Once);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllCustomersAsync_ShouldReturnEmptyCollection_WhenNoCustomersExist()
    {
        // Act
        var result = await _service.GetAllCustomersAsync();

        // Assert
        Assert.Empty(result);
        _mapperMock.Verify(m => m.ToCustomerDto(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetCustomerByIdAsync_ShouldReturnMappedDto_WhenCustomerExists()
    {
        // Arrange
        var user = CustomerTestData.CreateCustomer();

        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        var expectedDto = CustomerTestData.CreateCustomerDto(user.Id);
        _mapperMock.Setup(m => m.ToCustomerDto(It.Is<Customer>(u => u.Id == user.Id))).Returns(expectedDto);

        // Act
        var result = await _service.GetCustomerByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.CustomerId);
        _mapperMock.Verify(m => m.ToCustomerDto(user), Times.Once);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetCustomerByIdAsync_ShouldThrowKeyNotFoundException_WhenCustomerDoesNotExist()
    {
        // Act & Assert
        var missingId = Guid.NewGuid();
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetCustomerByIdAsync(missingId));
        Assert.Contains(missingId.ToString(), ex.Message);
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateCustomerAsync_ShouldCallMapperAndPersist_WhenValid()
    {
        // Arrange
        var user = CustomerTestData.CreateCustomer();

        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        var updateDto = CustomerTestData.CreateCustomerUpdateDto(email: "updated@domain.de", phoneNumber: null);
        _mapperMock.Setup(m => m.UpdateCustomerEntity(user, updateDto))
            .Callback<Customer, CustomerUpdateDto>((u, dto) => u.Email = dto.Email!);

        // Act
        await _service.UpdateCustomerAsync(user.Id, updateDto);

        // Assert
        var updated = await _context.Customers.FindAsync(user.Id);
        Assert.NotNull(updated);
        Assert.Equal("updated@domain.de", updated.Email);
        _mapperMock.Verify(m => m.UpdateCustomerEntity(user, updateDto), Times.Once);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateCustomerAsync_ShouldThrowInvalidOperationException_WhenNewEmailIsTakenByAnotherCustomer()
    {
        // Arrange
        var user1 = CustomerTestData.CreateCustomer(id: Guid.NewGuid(), email: "user1@domain.de");
        var user2 = CustomerTestData.CreateCustomer(id: Guid.NewGuid(), email: "user2@domain.de");

        _context.Customers.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var updateDto = CustomerTestData.CreateCustomerUpdateDto(email: "user2@domain.de");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateCustomerAsync(user1.Id, updateDto));
        Assert.Equal("Email is already in use.", ex.Message);
        Assert.Equal("user1@domain.de", (await _context.Customers.FindAsync(user1.Id))!.Email);
        _mapperMock.Verify(m => m.UpdateCustomerEntity(It.IsAny<Customer>(), It.IsAny<CustomerUpdateDto>()), Times.Never);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateCustomerAsync_ShouldThrowInvalidOperationException_WhenNewPhoneNumberIsTakenByAnotherCustomer()
    {
        // Arrange
        var user1 = CustomerTestData.CreateCustomer(id: Guid.NewGuid(), email: "user1@domain.de");
        var user2 = CustomerTestData.CreateCustomer(id: Guid.NewGuid(), email: "user2@domain.de", phoneNumber: "017012345678");
        _context.Customers.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var updateDto = CustomerTestData.CreateCustomerUpdateDto(email: null, phoneNumber: user2.PhoneNumber);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateCustomerAsync(user1.Id, updateDto));
        Assert.Equal("Phone number is already in use.", ex.Message);
        Assert.Null((await _context.Customers.FindAsync(user1.Id))!.PhoneNumber);
        _mapperMock.Verify(m => m.UpdateCustomerEntity(It.IsAny<Customer>(), It.IsAny<CustomerUpdateDto>()), Times.Never);
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteCustomerAsync_ShouldAnonymizeCustomer_WhenPasswordMatches()
    {
        // Arrange
        const string rawPassword = "CorrectPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var user = CustomerTestData.CreateCustomer(passwordHash: passwordHash);

        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.AnonymizeCustomerEntity(user))
            .Callback<Customer>(u =>
            {
                u.FirstName = "null";
                u.LastName = "null";
                u.Role = "anonymized_customer";
            });

        // Act
        await _service.DeleteCustomerAsync(user.Id, rawPassword);

        // Assert
        _mapperMock.Verify(m => m.AnonymizeCustomerEntity(user), Times.Once);
        Assert.Equal("anonymized_customer", (await _context.Customers.FindAsync(user.Id))!.Role);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteCustomerAsync_ShouldThrowInvalidOperationException_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = CustomerTestData.CreateCustomer(
            passwordHash: BCrypt.Net.BCrypt.HashPassword("CorrectPassword"));

        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteCustomerAsync(user.Id, "WrongPassword"));
        Assert.Equal("Invalid password.", ex.Message);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteCustomerAsync_ShouldThrowKeyNotFoundException_WhenCustomerDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.DeleteCustomerAsync(Guid.NewGuid(), "AnyPassword"));
    }

    #endregion

    #region LoginCustomerAsync Tests

    [Fact]
    [Trait("Action", "Login")]
    public async Task LoginCustomerAsync_ShouldReturnMappedDto_WhenCredentialsAreValid()
    {
        // Arrange
        const string password = "CorrectPassword123!";
        var user = CustomerTestData.CreateCustomer(
            email: "login@domain.de",
            passwordHash: BCrypt.Net.BCrypt.HashPassword(password));
        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        var expectedDto = CustomerTestData.CreateCustomerDto(user.Id, email: user.Email);
        _mapperMock.Setup(m => m.ToCustomerDto(user)).Returns(expectedDto);

        // Act
        var result = await _service.LoginCustomerAsync(new CustomerLoginDto(user.Email, password));

        // Assert
        Assert.Same(expectedDto, result);
        _mapperMock.Verify(m => m.ToCustomerDto(user), Times.Once);
    }

    [Fact]
    [Trait("Action", "Login")]
    public async Task LoginCustomerAsync_ShouldThrowUnauthorizedAccessException_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = CustomerTestData.CreateCustomer(
            email: "login@domain.de",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!"));
        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginCustomerAsync(new CustomerLoginDto(user.Email, "WrongPassword")));
        Assert.Equal("Invalid email or password.", ex.Message);
        _mapperMock.Verify(m => m.ToCustomerDto(It.IsAny<Customer>()), Times.Never);
    }

    #endregion

    #region IsEmailInUseAsync & IsPhoneNumberInUseAsync Tests

    [Fact]
    [Trait("Action", "Validation")]
    public async Task IsEmailInUseAsync_ShouldReturnTrue_WhenEmailExistsAndNotExcluded()
    {
        // Arrange
        var user = CustomerTestData.CreateCustomer(email: "exist@domain.de");

        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var inUse = await _service.IsEmailInUseAsync("exist@domain.de");
        var notInUseWhenExcluded = await _service.IsEmailInUseAsync("exist@domain.de", excludedCustomerId: user.Id);

        // Assert
        Assert.True(inUse);
        Assert.False(notInUseWhenExcluded);
    }

    [Fact]
    [Trait("Action", "Validation")]
    public async Task IsPhoneNumberInUseAsync_ShouldReturnTrue_WhenPhoneExistsAndNotExcluded()
    {
        // Arrange
        var user = CustomerTestData.CreateCustomer();
        user.PhoneNumber = "0123456789";

        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var inUse = await _service.IsPhoneNumberInUseAsync("0123456789");
        var notInUseWhenExcluded = await _service.IsPhoneNumberInUseAsync("0123456789", excludedCustomerId: user.Id);

        // Assert
        Assert.True(inUse);
        Assert.False(notInUseWhenExcluded);
    }

    #endregion
}