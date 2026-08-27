using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Suppliers;
using GroceryStore.Features.Suppliers.Interfaces;

using GroceryStore.Tests.TestData;
using SharedKernel.Security.Interfaces;

namespace GroceryStore.Tests.Features.Suppliers;

[Trait("Category", "Controller")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Suppliers")]
public class SupplierControllerTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<ISupplierMapper> _mapperMock;
    private readonly Mock<ILogger<SupplierService>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly SupplierService _service;
    private readonly SupplierController _controller;

    public SupplierControllerTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;

        _mapperMock = new Mock<ISupplierMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<SupplierService>>();
        _currentUserMock = new Mock<ICurrentUser>(MockBehavior.Strict);
        _tokenServiceMock = new Mock<ITokenService>(MockBehavior.Strict);

        _service = new SupplierService(_context, _mapperMock.Object, _loggerMock.Object);
        _controller = new SupplierController(_service, _currentUserMock.Object, _tokenServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetSuppliers & GetSupplierById Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetSuppliers_ShouldReturnOkWithSuppliersList()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = SupplierTestData.CreateSupplier(zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var expectedDto = SupplierTestData.CreateSupplierDto(supplier.Id);
        _mapperMock.Setup(m => m.ToSupplierDto(It.IsAny<Supplier>())).Returns(expectedDto);

        // Act
        var result = await _controller.GetSuppliers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var list = Assert.IsAssignableFrom<IEnumerable<SupplierDto>>(okResult.Value);
        Assert.Single(list);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetSupplierById_ShouldReturnOk_WhenCurrentSupplierExists()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = SupplierTestData.CreateSupplier(zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);

        var expectedDto = SupplierTestData.CreateSupplierDto(supplier.Id);
        _mapperMock.Setup(m => m.ToSupplierDto(It.Is<Supplier>(s => s.Id == supplier.Id))).Returns(expectedDto);

        // Act
        var result = await _controller.GetSupplierById();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedDto, okResult.Value);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetSupplierById_ShouldReturnNotFound_WhenCurrentSupplierDoesNotExist()
    {
        // Arrange
        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _controller.GetSupplierById();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region CreateSupplier Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateSupplier_ShouldReturnCreatedAtAction_WhenValid()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var registrationDto = SupplierTestData.CreateSupplierRegistrationDto();
        var entity = SupplierTestData.CreateSupplier(email: registrationDto.Email, zipCode: location.ZipCode);
        var createdDto = SupplierTestData.CreateSupplierDto(entity.Id, email: registrationDto.Email);

        _mapperMock.Setup(m => m.ToSupplierEntity(registrationDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToSupplierDto(entity)).Returns(createdDto);

        // Act
        var result = await _controller.CreateSupplier(registrationDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdAtResult.StatusCode);
        Assert.Equal(nameof(SupplierController.GetSupplierById), createdAtResult.ActionName);
        Assert.Same(createdDto, createdAtResult.Value);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateSupplier_ShouldReturnBadRequest_WhenEmailOrPhoneInUse()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var existing = SupplierTestData.CreateSupplier(email: "used@domain.de", zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Suppliers.Add(existing);
        await _context.SaveChangesAsync();

        var registrationDto = SupplierTestData.CreateSupplierRegistrationDto(email: "used@domain.de");
        var entity = SupplierTestData.CreateSupplier(email: "used@domain.de", zipCode: location.ZipCode);

        _mapperMock.Setup(m => m.ToSupplierEntity(registrationDto)).Returns(entity);

        // Act
        var result = await _controller.CreateSupplier(registrationDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Email is already in use.", badRequestResult.Value);
    }

    #endregion

    #region LoginSupplier Tests

    [Fact]
    [Trait("Action", "Login")]
    public async Task LoginSupplier_ShouldReturnOkWithToken_WhenCredentialsAreValid()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var rawPassword = "CorrectPass123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var supplier = SupplierTestData.CreateSupplier(
            email: "login@domain.de",
            passwordHash: passwordHash,
            zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var supplierDto = SupplierTestData.CreateSupplierDto(supplier.Id, email: supplier.Email, role: "supplier");
        _mapperMock.Setup(m => m.ToSupplierDto(It.Is<Supplier>(s => s.Id == supplier.Id))).Returns(supplierDto);
        _tokenServiceMock.Setup(t => t.GenerateToken(supplier.Id, "login@domain.de", "supplier", null))
            .Returns("generated_jwt_token_xyz");

        var loginDto = new SupplierLoginDto("login@domain.de", rawPassword);

        // Act
        var result = await _controller.LoginSupplier(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var authResponse = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("generated_jwt_token_xyz", authResponse.Token);
        Assert.Equal(supplier.Id, authResponse.SupplierId);
        Assert.Equal("supplier", authResponse.Role);
    }

    [Fact]
    [Trait("Action", "Login")]
    public async Task LoginSupplier_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var loginDto = new SupplierLoginDto("nonexistent@domain.de", "WrongPass");

        // Act
        var result = await _controller.LoginSupplier(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        Assert.Equal("Invalid email or password.", unauthorizedResult.Value);
    }

    #endregion

    #region UpdateSupplier Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateSupplier_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = SupplierTestData.CreateSupplier(zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);
        var updateDto = SupplierTestData.CreateSupplierUpdateDto(email: "newemail@domain.de");
        _mapperMock.Setup(m => m.UpdateSupplierEntity(supplier, updateDto));

        // Act
        var result = await _controller.UpdateSupplier(updateDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateSupplier_ShouldReturnNotFound_WhenSupplierMissing()
    {
        // Arrange
        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        var updateDto = SupplierTestData.CreateSupplierUpdateDto();

        // Act
        var result = await _controller.UpdateSupplier(updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region DeleteSupplier Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteSupplier_ShouldReturnNoContent_WhenPasswordIsCorrect()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var rawPassword = "CorrectPass123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var supplier = SupplierTestData.CreateSupplier(passwordHash: passwordHash, zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);
        _mapperMock.Setup(m => m.AnonymizeSupplierEntity(supplier));

        // Act
        var result = await _controller.DeleteSupplier(new SupplierActionRequest(rawPassword));

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteSupplier_ShouldReturnBadRequest_WhenPasswordIsIncorrect()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = SupplierTestData.CreateSupplier(
            passwordHash: BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
            zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);

        // Act
        var result = await _controller.DeleteSupplier(new SupplierActionRequest("WrongPassword"));

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Invalid password.", badRequestResult.Value);
    }

    #endregion
}