using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Allergens;
using GroceryStore.Features.Allergens.Interfaces;

using GroceryStore.Tests.TestData;
using SharedKernel.Security.Interfaces;

namespace GroceryStore.Tests.Features.Allergens;

[Trait("Category", "Controller")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Allergens")]
public class AllergenControllerTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<IAllergenMapper> _mapperMock;
    private readonly Mock<ILogger<AllergenService>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly AllergenService _service;
    private readonly AllergenController _controller;

    public AllergenControllerTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;

        _mapperMock = new Mock<IAllergenMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<AllergenService>>();
        _currentUserMock = new Mock<ICurrentUser>(MockBehavior.Strict);

        _service = new AllergenService(_context, _mapperMock.Object, _loggerMock.Object);
        _controller = new AllergenController(_service, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetAllAllergens Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllAllergens_ShouldReturnOkWithAllergens()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var allergen = AllergenTestData.CreateAllergen(0, "Gluten", supplier.Id);

        _context.Suppliers.Add(supplier);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        var expectedDto = AllergenTestData.CreateAllergenDto("Gluten", supplier.Id);
        _mapperMock.Setup(m => m.ToAllergenDto(It.IsAny<Allergen>())).Returns(expectedDto);

        // Act
        var result = await _controller.GetAllAllergens();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<AllergenDto>>(okResult.Value);
        Assert.Single(returnedItems);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllAllergens_ShouldReturnOkWithEmptyCollection_WhenNoAllergensExist()
    {
        // Act
        var result = await _controller.GetAllAllergens();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<AllergenDto>>(okResult.Value);
        Assert.Empty(returnedItems);
    }

    #endregion

    #region GetAllergenById Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllergenById_ShouldReturnOk_WhenAllergenExists()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var allergen = AllergenTestData.CreateAllergen(0, "Laktose", supplier.Id);

        _context.Suppliers.Add(supplier);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        var expectedDto = AllergenTestData.CreateAllergenDto("Laktose", supplier.Id);
        _mapperMock.Setup(m => m.ToAllergenDto(It.Is<Allergen>(a => a.Id == allergen.Id))).Returns(expectedDto);

        // Act
        var result = await _controller.GetAllergenById(allergen.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedDto, okResult.Value);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllergenById_ShouldReturnNotFound_WhenAllergenDoesNotExist()
    {
        // Act
        var result = await _controller.GetAllergenById(404);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region CreateAllergen Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateAllergen_ShouldReturnCreatedAtAction_WhenValid()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = AllergenTestData.CreateAllergenCreateDto("Sellerie", supplier.Id);
        var entity = new Allergen { Name = "Sellerie", SupplierId = supplier.Id };
        var createdDto = AllergenTestData.CreateAllergenDto("Sellerie", supplier.Id);

        _mapperMock.Setup(m => m.ToAllergenEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToAllergenDto(It.Is<Allergen>(a => a.Name == "Sellerie"))).Returns(createdDto);

        // Act
        var result = await _controller.CreateAllergen(createDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, createdAtResult.StatusCode);
        Assert.Equal(nameof(AllergenController.GetAllergenById), createdAtResult.ActionName);
        Assert.Equal(createdDto.AllergenId, createdAtResult.RouteValues!["allergenId"]);
        Assert.Same(createdDto, createdAtResult.Value);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateAllergen_ShouldReturnBadRequest_WhenMappingFails()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = AllergenTestData.CreateAllergenCreateDto(supplierId: supplier.Id);
        var entity = new Allergen { Name = createDto.Name, SupplierId = createDto.SupplierId };

        _mapperMock.Setup(m => m.ToAllergenEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToAllergenDto(It.IsAny<Allergen>())).Returns((AllergenDto)null!);

        // Act
        var result = await _controller.CreateAllergen(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    #endregion

    #region UpdateAllergen Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateAllergen_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var allergen = AllergenTestData.CreateAllergen(0, "Alt", supplier.Id);

        _context.Suppliers.Add(supplier);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        var updateDto = AllergenTestData.CreateAllergenUpdateDto("Neu");
        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);
        _mapperMock.Setup(m => m.UpdateAllergenEntity(allergen, updateDto))
            .Callback<Allergen, AllergenUpdateDto>((entity, dto) => entity.Name = dto.Name!);

        // Act
        var result = await _controller.UpdateAllergen(allergen.Id, updateDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        Assert.Equal("Neu", (await _context.Allergens.FindAsync(allergen.Id))!.Name);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateAllergen_ShouldReturnNotFound_WhenAllergenMissing()
    {
        // Arrange
        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        var updateDto = AllergenTestData.CreateAllergenUpdateDto();

        // Act
        var result = await _controller.UpdateAllergen(999, updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateAllergen_ShouldReturnForbid_WhenSupplierIsNotOwner()
    {
        // Arrange
        var owner = GroceryStoreTestData.CreateSupplier();
        var strangerId = Guid.NewGuid();
        var allergen = AllergenTestData.CreateAllergen(0, "Gluten", owner.Id);

        _context.Suppliers.Add(owner);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(strangerId);
        var updateDto = AllergenTestData.CreateAllergenUpdateDto();

        // Act
        var result = await _controller.UpdateAllergen(allergen.Id, updateDto);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.NotNull(forbidResult);
        Assert.Equal("Gluten", (await _context.Allergens.FindAsync(allergen.Id))!.Name);
    }

    #endregion

    #region DeleteAllergen Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteAllergen_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var allergen = AllergenTestData.CreateAllergen(0, "Senf", supplier.Id);

        _context.Suppliers.Add(supplier);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);

        // Act
        var result = await _controller.DeleteAllergen(allergen.Id);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        Assert.Null(await _context.Allergens.FindAsync(allergen.Id));
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteAllergen_ShouldReturnNotFound_WhenAllergenMissing()
    {
        // Arrange
        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _controller.DeleteAllergen(500);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteAllergen_ShouldReturnForbid_WhenSupplierIsNotOwner()
    {
        // Arrange
        var owner = GroceryStoreTestData.CreateSupplier();
        var strangerId = Guid.NewGuid();
        var allergen = AllergenTestData.CreateAllergen(0, "Lupine", owner.Id);

        _context.Suppliers.Add(owner);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(strangerId);

        // Act
        var result = await _controller.DeleteAllergen(allergen.Id);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.NotNull(forbidResult);
        Assert.NotNull(await _context.Allergens.FindAsync(allergen.Id));
    }

    #endregion
}