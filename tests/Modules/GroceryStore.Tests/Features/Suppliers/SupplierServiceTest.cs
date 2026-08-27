using Moq;

using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Suppliers;
using GroceryStore.Features.Suppliers.Interfaces;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Suppliers;

[Trait("Category", "Service")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Suppliers")]
public class SupplierServiceTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<ISupplierMapper> _mapperMock;
    private readonly Mock<ILogger<SupplierService>> _loggerMock;
    private readonly SupplierService _service;

    public SupplierServiceTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;
        _mapperMock = new Mock<ISupplierMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<SupplierService>>();

        _service = new SupplierService(_context, _mapperMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region CreateSupplierAsync Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateSupplierAsync_ShouldPersistSupplierAndReturnDto_WhenValid()
    {
        // Arrange
        var registrationDto = SupplierTestData.CreateSupplierRegistrationDto("Neu GmbH", "neu@domain.de");
        var entityToInsert = SupplierTestData.CreateSupplier(companyName: "Neu GmbH", email: "neu@domain.de", zipCode: registrationDto.ZipCode);
        var expectedDto = SupplierTestData.CreateSupplierDto(entityToInsert.Id, "Neu GmbH", "neu@domain.de");

        _mapperMock.Setup(m => m.ToSupplierEntity(registrationDto)).Returns(entityToInsert);
        _mapperMock.Setup(m => m.ToSupplierDto(entityToInsert)).Returns(expectedDto);

        // Act
        var result = await _service.CreateSupplierAsync(registrationDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Neu GmbH", result.CompanyName);
        Assert.Equal("neu@domain.de", result.Email);

        var persisted = await _context.Suppliers.FindAsync(entityToInsert.Id);
        Assert.NotNull(persisted);

        _mapperMock.Verify(m => m.ToSupplierEntity(registrationDto), Times.Once);
        _mapperMock.Verify(m => m.ToSupplierDto(entityToInsert), Times.Once);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateSupplierAsync_ShouldThrowInvalidOperationException_WhenEmailIsAlreadyInUse()
    {
        // Arrange
        var existingSupplier = GroceryStoreTestData.CreateSupplier(email: "duplicate@domain.de");

        _context.Suppliers.Add(existingSupplier);
        await _context.SaveChangesAsync();

        var registrationDto = SupplierTestData.CreateSupplierRegistrationDto(email: "duplicate@domain.de");
        var entityToInsert = SupplierTestData.CreateSupplier(email: "duplicate@domain.de");

        _mapperMock.Setup(m => m.ToSupplierEntity(registrationDto)).Returns(entityToInsert);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateSupplierAsync(registrationDto));
        Assert.Equal("Email is already in use.", ex.Message);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateSupplierAsync_ShouldThrowInvalidOperationException_WhenPhoneNumberIsAlreadyInUse()
    {
        // Arrange
        var existingSupplier = SupplierTestData.CreateSupplier(email: "first@domain.de");
        existingSupplier.PhoneNumber = "0151999999";

        _context.Suppliers.Add(existingSupplier);
        await _context.SaveChangesAsync();

        var registrationDto = SupplierTestData.CreateSupplierRegistrationDto(email: "second@domain.de");
        var entityToInsert = SupplierTestData.CreateSupplier(email: "second@domain.de");
        entityToInsert.PhoneNumber = "0151999999";

        _mapperMock.Setup(m => m.ToSupplierEntity(registrationDto)).Returns(entityToInsert);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateSupplierAsync(registrationDto));
        Assert.Equal("Phone number is already in use.", ex.Message);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateSupplierAsync_ShouldThrowInvalidOperationException_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var registrationDto = SupplierTestData.CreateSupplierRegistrationDto(
            password: "SecurePassword123!",
            confirmPassword: "DifferentPassword123!");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateSupplierAsync(registrationDto));
        Assert.Equal("Passwords do not match.", ex.Message);
        _mapperMock.Verify(m => m.ToSupplierEntity(It.IsAny<SupplierRegistrationDto>()), Times.Never);
    }

    #endregion

    #region GetAllSuppliersAsync & GetSupplierByIdAsync Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllSuppliersAsync_ShouldReturnAllSuppliers()
    {
        // Arrange
        var supplier1 = SupplierTestData.CreateSupplier(id: Guid.NewGuid(), email: "sup1@domain.de");
        var supplier2 = SupplierTestData.CreateSupplier(id: Guid.NewGuid(), email: "sup2@domain.de");

        _context.Suppliers.AddRange(supplier1, supplier2);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.ToSupplierDto(It.Is<Supplier>(s => s.Id == supplier1.Id)))
            .Returns(SupplierTestData.CreateSupplierDto(supplier1.Id, email: "sup1@domain.de"));
        _mapperMock.Setup(m => m.ToSupplierDto(It.Is<Supplier>(s => s.Id == supplier2.Id)))
            .Returns(SupplierTestData.CreateSupplierDto(supplier2.Id, email: "sup2@domain.de"));

        // Act
        var result = await _service.GetAllSuppliersAsync();

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, s => s.Email == "sup1@domain.de");
        Assert.Contains(list, s => s.Email == "sup2@domain.de");
        _mapperMock.Verify(m => m.ToSupplierDto(supplier1), Times.Once);
        _mapperMock.Verify(m => m.ToSupplierDto(supplier2), Times.Once);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllSuppliersAsync_ShouldReturnEmptyCollection_WhenNoSuppliersExist()
    {
        // Act
        var result = await _service.GetAllSuppliersAsync();

        // Assert
        Assert.Empty(result);
        _mapperMock.Verify(m => m.ToSupplierDto(It.IsAny<Supplier>()), Times.Never);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetSupplierByIdAsync_ShouldReturnMappedDto_WhenSupplierExists()
    {
        // Arrange
        var supplier = SupplierTestData.CreateSupplier();

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var expectedDto = SupplierTestData.CreateSupplierDto(supplier.Id);
        _mapperMock.Setup(m => m.ToSupplierDto(It.Is<Supplier>(s => s.Id == supplier.Id))).Returns(expectedDto);

        // Act
        var result = await _service.GetSupplierByIdAsync(supplier.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(supplier.Id, result.SupplierId);
        _mapperMock.Verify(m => m.ToSupplierDto(supplier), Times.Once);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetSupplierByIdAsync_ShouldThrowKeyNotFoundException_WhenSupplierDoesNotExist()
    {
        // Act & Assert
        var missingId = Guid.NewGuid();
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetSupplierByIdAsync(missingId));
        Assert.Contains(missingId.ToString(), ex.Message);
    }

    #endregion

    #region UpdateSupplierAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateSupplierAsync_ShouldCallMapperAndPersist_WhenValid()
    {
        // Arrange
        var supplier = SupplierTestData.CreateSupplier();

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var updateDto = SupplierTestData.CreateSupplierUpdateDto(email: "updated@domain.de", phoneNumber: null);
        _mapperMock.Setup(m => m.UpdateSupplierEntity(supplier, updateDto))
            .Callback<Supplier, SupplierUpdateDto>((s, dto) => s.Email = dto.Email!);

        // Act
        await _service.UpdateSupplierAsync(supplier.Id, updateDto);

        // Assert
        var updated = await _context.Suppliers.FindAsync(supplier.Id);
        Assert.NotNull(updated);
        Assert.Equal("updated@domain.de", updated.Email);
        _mapperMock.Verify(m => m.UpdateSupplierEntity(supplier, updateDto), Times.Once);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateSupplierAsync_ShouldThrowInvalidOperationException_WhenNewEmailIsTakenByAnotherSupplier()
    {
        // Arrange
        var supplier1 = SupplierTestData.CreateSupplier(id: Guid.NewGuid(), email: "sup1@domain.de");
        var supplier2 = SupplierTestData.CreateSupplier(id: Guid.NewGuid(), email: "sup2@domain.de");

        _context.Suppliers.AddRange(supplier1, supplier2);
        await _context.SaveChangesAsync();

        var updateDto = SupplierTestData.CreateSupplierUpdateDto(email: "sup2@domain.de");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateSupplierAsync(supplier1.Id, updateDto));
        Assert.Equal("Email is already in use.", ex.Message);
        Assert.Equal("sup1@domain.de", (await _context.Suppliers.FindAsync(supplier1.Id))!.Email);
        _mapperMock.Verify(m => m.UpdateSupplierEntity(It.IsAny<Supplier>(), It.IsAny<SupplierUpdateDto>()), Times.Never);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateSupplierAsync_ShouldThrowInvalidOperationException_WhenNewPhoneNumberIsTakenByAnotherSupplier()
    {
        // Arrange
        var supplier1 = SupplierTestData.CreateSupplier(id: Guid.NewGuid(), email: "sup1@domain.de");
        var supplier2 = SupplierTestData.CreateSupplier(id: Guid.NewGuid(), email: "sup2@domain.de", phoneNumber: "017012345678");
        _context.Suppliers.AddRange(supplier1, supplier2);
        await _context.SaveChangesAsync();

        var updateDto = SupplierTestData.CreateSupplierUpdateDto(email: null, phoneNumber: supplier2.PhoneNumber);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateSupplierAsync(supplier1.Id, updateDto));
        Assert.Equal("Phone number is already in use.", ex.Message);
        Assert.Null((await _context.Suppliers.FindAsync(supplier1.Id))!.PhoneNumber);
        _mapperMock.Verify(m => m.UpdateSupplierEntity(It.IsAny<Supplier>(), It.IsAny<SupplierUpdateDto>()), Times.Never);
    }

    #endregion

    #region DeleteSupplierAsync Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteSupplierAsync_ShouldAnonymizeSupplier_WhenPasswordMatches()
    {
        // Arrange
        var rawPassword = "CorrectPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);
        var supplier = SupplierTestData.CreateSupplier(passwordHash: hashedPassword);

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.AnonymizeSupplierEntity(supplier))
            .Callback<Supplier>(s =>
            {
                s.CompanyName = "Anonymized Supplier";
                s.Role = "anonymized_supplier";
            });

        // Act
        await _service.DeleteSupplierAsync(supplier.Id, rawPassword);

        // Assert
        _mapperMock.Verify(m => m.AnonymizeSupplierEntity(supplier), Times.Once);
        Assert.Equal("anonymized_supplier", (await _context.Suppliers.FindAsync(supplier.Id))!.Role);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteSupplierAsync_ShouldThrowInvalidOperationException_WhenPasswordIsIncorrect()
    {
        // Arrange
        var supplier = SupplierTestData.CreateSupplier(
            passwordHash: BCrypt.Net.BCrypt.HashPassword("CorrectPassword"));

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteSupplierAsync(supplier.Id, "WrongPassword"));
        Assert.Equal("Invalid password.", ex.Message);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteSupplierAsync_ShouldThrowKeyNotFoundException_WhenSupplierDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.DeleteSupplierAsync(Guid.NewGuid(), "AnyPassword"));
    }

    #endregion

    #region LoginSupplierAsync Tests

    [Fact]
    [Trait("Action", "Login")]
    public async Task LoginSupplierAsync_ShouldReturnMappedDto_WhenCredentialsAreValid()
    {
        // Arrange
        const string password = "CorrectPassword123!";
        var supplier = SupplierTestData.CreateSupplier(
            email: "login@domain.de",
            passwordHash: BCrypt.Net.BCrypt.HashPassword(password));
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var expectedDto = SupplierTestData.CreateSupplierDto(supplier.Id, email: supplier.Email);
        _mapperMock.Setup(m => m.ToSupplierDto(supplier)).Returns(expectedDto);

        // Act
        var result = await _service.LoginSupplierAsync(new SupplierLoginDto(supplier.Email, password));

        // Assert
        Assert.Same(expectedDto, result);
        _mapperMock.Verify(m => m.ToSupplierDto(supplier), Times.Once);
    }

    [Fact]
    [Trait("Action", "Login")]
    public async Task LoginSupplierAsync_ShouldThrowUnauthorizedAccessException_WhenPasswordIsInvalid()
    {
        // Arrange
        var supplier = SupplierTestData.CreateSupplier(
            email: "login@domain.de",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!"));
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginSupplierAsync(new SupplierLoginDto(supplier.Email, "WrongPassword")));
        Assert.Equal("Invalid email or password.", ex.Message);
        _mapperMock.Verify(m => m.ToSupplierDto(It.IsAny<Supplier>()), Times.Never);
    }

    #endregion

    #region IsEmailInUseAsync & IsPhoneNumberInUseAsync Tests

    [Fact]
    [Trait("Action", "Validation")]
    public async Task IsEmailInUseAsync_ShouldReturnTrue_WhenEmailExistsAndNotExcluded()
    {
        // Arrange
        var supplier = SupplierTestData.CreateSupplier(email: "exist@domain.de");

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Act
        var inUse = await _service.IsEmailInUseAsync("exist@domain.de");
        var notInUseWhenExcluded = await _service.IsEmailInUseAsync("exist@domain.de", excludedSupplierId: supplier.Id);

        // Assert
        Assert.True(inUse);
        Assert.False(notInUseWhenExcluded);
    }

    [Fact]
    [Trait("Action", "Validation")]
    public async Task IsPhoneNumberInUseAsync_ShouldReturnTrue_WhenPhoneExistsAndNotExcluded()
    {
        // Arrange
        var supplier = SupplierTestData.CreateSupplier();
        supplier.PhoneNumber = "0123456789";

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Act
        var inUse = await _service.IsPhoneNumberInUseAsync("0123456789");
        var notInUseWhenExcluded = await _service.IsPhoneNumberInUseAsync("0123456789", excludedSupplierId: supplier.Id);

        // Assert
        Assert.True(inUse);
        Assert.False(notInUseWhenExcluded);
    }

    #endregion
}