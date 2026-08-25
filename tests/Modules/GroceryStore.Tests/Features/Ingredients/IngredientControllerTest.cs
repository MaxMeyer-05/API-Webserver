using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Ingredients;
using GroceryStore.Features.Ingredients.Interfaces;

using GroceryStore.Tests.TestData;
using SharedKernel.Security.Interfaces;

namespace GroceryStore.Tests.Features.Ingredients;

[Trait("Category", "Controller")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Ingredients")]
public class IngredientControllerTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<IIngredientMapper> _mapperMock;
    private readonly Mock<ILogger<IngredientService>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly IngredientService _service;
    private readonly IngredientController _controller;

    public IngredientControllerTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;

        _mapperMock = new Mock<IIngredientMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<IngredientService>>();
        _currentUserMock = new Mock<ICurrentUser>(MockBehavior.Strict);

        _service = new IngredientService(_context, _mapperMock.Object, _loggerMock.Object);
        _controller = new IngredientController(_service, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetAllIngredients Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllIngredients_ShouldReturnOkWithIngredients()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Bio-Milch");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        var expectedDto = IngredientTestData.CreateIngredientDto(ingredient.Id, "Bio-Milch", supplierId: supplier.Id);
        _mapperMock.Setup(m => m.ToIngredientDto(It.IsAny<Ingredient>())).Returns(expectedDto);

        // Act
        var result = await _controller.GetAllIngredients();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var list = Assert.IsAssignableFrom<IEnumerable<IngredientDto>>(okResult.Value);
        Assert.Single(list);
    }

    #endregion

    #region GetIngredientById Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetIngredientById_ShouldReturnOk_WhenIngredientExists()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Dinkelmehl");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        var expectedDto = IngredientTestData.CreateIngredientDto(ingredient.Id, "Dinkelmehl", supplierId: supplier.Id);
        _mapperMock.Setup(m => m.ToIngredientDto(It.Is<Ingredient>(i => i.Id == ingredient.Id))).Returns(expectedDto);

        // Act
        var result = await _controller.GetIngredientById(ingredient.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedDto, okResult.Value);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetIngredientById_ShouldReturnNotFound_WhenIngredientDoesNotExist()
    {
        // Act
        var result = await _controller.GetIngredientById(404);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region CreateIngredient Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateIngredient_ShouldReturnCreatedAtAction_WhenValid()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = IngredientTestData.CreateIngredientCreateDto("Zimt", "g", 0.89m, 20, supplier.Id);
        var entity = new Ingredient { Name = "Zimt", Unit = "g", NetPrice = 0.89m, Stock = 20, SupplierId = supplier.Id };
        var createdDto = IngredientTestData.CreateIngredientDto(1, "Zimt", "g", 0.89m, 20, supplier.Id);

        _mapperMock.Setup(m => m.ToIngredientEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToIngredientDto(It.Is<Ingredient>(i => i.Name == "Zimt"))).Returns(createdDto);

        // Act
        var result = await _controller.CreateIngredient(createDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, createdAtResult.StatusCode);
        Assert.Equal(nameof(IngredientController.GetIngredientById), createdAtResult.ActionName);
        Assert.Same(createdDto, createdAtResult.Value);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateIngredient_ShouldReturnBadRequest_WhenMappingFails()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = IngredientTestData.CreateIngredientCreateDto(supplierId: supplier.Id);
        var entity = new Ingredient { Name = createDto.Name, Unit = createDto.Unit, SupplierId = createDto.SupplierId };

        _mapperMock.Setup(m => m.ToIngredientEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToIngredientDto(It.IsAny<Ingredient>())).Returns((IngredientDto)null!);

        // Act
        var result = await _controller.CreateIngredient(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    #endregion

    #region AddAllergenToIngredient Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task AddAllergenToIngredient_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Weizenbrot");
        var allergen = IngredientTestData.CreateAllergen(1, "Gluten", supplier.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);

        // Act
        var result = await _controller.AddAllergenToIngredient(ingredient.Id, allergen.Id);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task AddAllergenToIngredient_ShouldReturnForbid_WhenSupplierIsNotOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var owner = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(owner.Id, "Nussriegel");
        var allergen = IngredientTestData.CreateAllergen(1, "Nüsse", owner.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(owner);
        _context.Ingredients.Add(ingredient);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _controller.AddAllergenToIngredient(ingredient.Id, allergen.Id);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.NotNull(forbidResult);
    }

    #endregion

    #region UpdateIngredient Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateIngredient_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Roggenmehl");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        var updateDto = IngredientTestData.CreateIngredientUpdateDto("Roggen-Vollkorn");
        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);
        _mapperMock.Setup(m => m.UpdateIngredientEntity(ingredient, updateDto))
            .Callback<Ingredient, IngredientUpdateDto>((entity, dto) => entity.Name = dto.Name!);

        // Act
        var result = await _controller.UpdateIngredient(ingredient.Id, updateDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateIngredient_ShouldReturnForbid_WhenSupplierIsNotOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var owner = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(owner.Id, "Vanille");

        _context.Locations.Add(location);
        _context.Suppliers.Add(owner);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        var updateDto = IngredientTestData.CreateIngredientUpdateDto();

        // Act
        var result = await _controller.UpdateIngredient(ingredient.Id, updateDto);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.NotNull(forbidResult);
    }

    #endregion

    #region DeleteIngredient Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteIngredient_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Hefe");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);

        // Act
        var result = await _controller.DeleteIngredient(ingredient.Id);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteIngredient_ShouldReturnNotFound_WhenIngredientMissing()
    {
        // Arrange
        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _controller.DeleteIngredient(404);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region RemoveAllergenFromIngredient Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task RemoveAllergenFromIngredient_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var allergen = IngredientTestData.CreateAllergen(1, "Laktose", supplier.Id);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Quark");
        ingredient.Allergens.Add(allergen);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);

        // Act
        var result = await _controller.RemoveAllergenFromIngredient(ingredient.Id, allergen.Id);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task RemoveAllergenFromIngredient_ShouldReturnBadRequest_WhenRelationDoesNotExist()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var allergen = IngredientTestData.CreateAllergen(1, "Sesam", supplier.Id);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Dinkelreis");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);

        // Act
        var result = await _controller.RemoveAllergenFromIngredient(ingredient.Id, allergen.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    #endregion
}