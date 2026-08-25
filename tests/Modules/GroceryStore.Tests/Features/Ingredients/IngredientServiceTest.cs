using Moq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Ingredients;
using GroceryStore.Features.Ingredients.Interfaces;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Ingredients;

[Trait("Category", "Service")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Ingredients")]
public class IngredientServiceTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<IIngredientMapper> _mapperMock;
    private readonly Mock<ILogger<IngredientService>> _loggerMock;
    private readonly IngredientService _service;

    public IngredientServiceTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;
        _mapperMock = new Mock<IIngredientMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<IngredientService>>();

        _service = new IngredientService(_context, _mapperMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region CreateIngredientAsync Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateIngredientAsync_ShouldPersistEntityAndReturnDto()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = IngredientTestData.CreateIngredientCreateDto("Butter", "kg", 2.29m, 40, supplier.Id);
        var entityToInsert = new Ingredient
        {
            Name = "Butter",
            Unit = "kg",
            NetPrice = 2.29m,
            Stock = 40,
            SupplierId = supplier.Id
        };
        var expectedDto = IngredientTestData.CreateIngredientDto(1, "Butter", "kg", 2.29m, 40, supplier.Id, supplier.CompanyName);

        _mapperMock.Setup(m => m.ToIngredientEntity(createDto)).Returns(entityToInsert);
        _mapperMock.Setup(m => m.ToIngredientDto(It.Is<Ingredient>(i => i.Name == "Butter"))).Returns(expectedDto);

        // Act
        var result = await _service.CreateIngredientAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Butter", result.Name);

        var persisted = await _context.Ingredients.FirstOrDefaultAsync(i => i.Name == "Butter");
        Assert.NotNull(persisted);
        Assert.NotEqual(0, persisted.Id);

        _mapperMock.Verify(m => m.ToIngredientEntity(createDto), Times.Once);
        _mapperMock.Verify(m => m.ToIngredientDto(entityToInsert), Times.Once);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateIngredientAsync_ShouldThrowInvalidOperationException_WhenDtoMappingFails()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = IngredientTestData.CreateIngredientCreateDto(supplierId: supplier.Id);
        var entity = new Ingredient { Name = createDto.Name, Unit = createDto.Unit, SupplierId = supplier.Id };

        _mapperMock.Setup(m => m.ToIngredientEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToIngredientDto(entity)).Returns((IngredientDto)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateIngredientAsync(createDto));
        Assert.Equal("Failed to map the created ingredient entity to DTO", ex.Message);
    }

    #endregion

    #region GetAllIngredientsAsync & GetIngredientByIdAsync Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllIngredientsAsync_ShouldReturnAllMappedIngredients()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient1 = GroceryStoreTestData.CreateIngredient(supplier.Id, "Apfel");
        var ingredient2 = GroceryStoreTestData.CreateIngredient(supplier.Id, "Birne");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.AddRange(ingredient1, ingredient2);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.ToIngredientDto(It.Is<Ingredient>(i => i.Name == "Apfel")))
            .Returns(IngredientTestData.CreateIngredientDto(ingredient1.Id, "Apfel", supplierId: supplier.Id));
        _mapperMock.Setup(m => m.ToIngredientDto(It.Is<Ingredient>(i => i.Name == "Birne")))
            .Returns(IngredientTestData.CreateIngredientDto(ingredient2.Id, "Birne", supplierId: supplier.Id));

        // Act
        var result = await _service.GetAllIngredientsAsync();

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, i => i.Name == "Apfel");
        Assert.Contains(list, i => i.Name == "Birne");
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetIngredientByIdAsync_ShouldReturnMappedDto_WhenIngredientExists()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Zucker");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        var expectedDto = IngredientTestData.CreateIngredientDto(ingredient.Id, "Zucker", supplierId: supplier.Id);
        _mapperMock.Setup(m => m.ToIngredientDto(It.Is<Ingredient>(i => i.Id == ingredient.Id))).Returns(expectedDto);

        // Act
        var result = await _service.GetIngredientByIdAsync(ingredient.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Zucker", result.Name);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetIngredientByIdAsync_ShouldThrowKeyNotFoundException_WhenIngredientDoesNotExist()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetIngredientByIdAsync(999));
        Assert.Contains("999", ex.Message);
    }

    #endregion

    #region UpdateIngredientAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateIngredientAsync_ShouldUpdate_WhenSupplierIsOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Mehl");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        var updateDto = IngredientTestData.CreateIngredientUpdateDto("Dinkelmehl");
        _mapperMock.Setup(m => m.UpdateIngredientEntity(ingredient, updateDto))
            .Callback<Ingredient, IngredientUpdateDto>((entity, dto) => entity.Name = dto.Name!);

        // Act
        await _service.UpdateIngredientAsync(ingredient.Id, supplier.Id, updateDto);

        // Assert
        var updated = await _context.Ingredients.FindAsync(ingredient.Id);
        Assert.NotNull(updated);
        Assert.Equal("Dinkelmehl", updated.Name);
        _mapperMock.Verify(m => m.UpdateIngredientEntity(ingredient, updateDto), Times.Once);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateIngredientAsync_ShouldThrowUnauthorizedAccessException_WhenSupplierIsNotOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var owner = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var strangerId = Guid.NewGuid();
        var ingredient = GroceryStoreTestData.CreateIngredient(owner.Id, "Salz");

        _context.Locations.Add(location);
        _context.Suppliers.Add(owner);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        var updateDto = IngredientTestData.CreateIngredientUpdateDto("Meersalz");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.UpdateIngredientAsync(ingredient.Id, strangerId, updateDto));
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateIngredientAsync_ShouldThrowKeyNotFoundException_WhenIngredientMissing()
    {
        // Arrange
        var updateDto = IngredientTestData.CreateIngredientUpdateDto();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateIngredientAsync(404, Guid.NewGuid(), updateDto));
    }

    #endregion

    #region DeleteIngredientAsync Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteIngredientAsync_ShouldRemoveEntity_WhenSupplierIsOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Pfeffer");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteIngredientAsync(ingredient.Id, supplier.Id);

        // Assert
        var exists = await _context.Ingredients.AnyAsync(i => i.Id == ingredient.Id);
        Assert.False(exists);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteIngredientAsync_ShouldThrowUnauthorizedAccessException_WhenSupplierIsNotOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var owner = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(owner.Id, "Oregano");

        _context.Locations.Add(location);
        _context.Suppliers.Add(owner);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.DeleteIngredientAsync(ingredient.Id, Guid.NewGuid()));
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteIngredientAsync_ShouldThrowKeyNotFoundException_WhenIngredientMissing()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.DeleteIngredientAsync(999, Guid.NewGuid()));
    }

    #endregion

    #region AddAllergenToIngredientAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task AddAllergenToIngredientAsync_ShouldAddRelation_WhenValidAndOwner()
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

        // Act
        await _service.AddAllergenToIngredientAsync(ingredient.Id, allergen.Id, supplier.Id);

        // Assert
        var updated = await _context.Ingredients.Include(i => i.Allergens).FirstAsync(i => i.Id == ingredient.Id);
        Assert.Single(updated.Allergens);
        Assert.Equal("Gluten", updated.Allergens.First().Name);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task AddAllergenToIngredientAsync_ShouldThrowUnauthorizedAccessException_WhenSupplierIsNotOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var owner = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(owner.Id, "Nusskuchen");
        var allergen = IngredientTestData.CreateAllergen(1, "Nüsse", owner.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(owner);
        _context.Ingredients.Add(ingredient);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.AddAllergenToIngredientAsync(ingredient.Id, allergen.Id, Guid.NewGuid()));
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task AddAllergenToIngredientAsync_ShouldThrowKeyNotFoundException_WhenIngredientNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.AddAllergenToIngredientAsync(999, 1, Guid.NewGuid()));
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task AddAllergenToIngredientAsync_ShouldThrowKeyNotFoundException_WhenAllergenNotFound()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Reis");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.AddAllergenToIngredientAsync(ingredient.Id, 888, supplier.Id));
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task AddAllergenToIngredientAsync_ShouldThrowInvalidOperationException_WhenAlreadyAssigned()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var allergen = IngredientTestData.CreateAllergen(1, "Soja", supplier.Id);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Tofu");
        ingredient.Allergens.Add(allergen);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AddAllergenToIngredientAsync(ingredient.Id, allergen.Id, supplier.Id));
        Assert.Contains("already has allergen", ex.Message);
    }

    #endregion

    #region RemoveAllergenFromIngredientAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task RemoveAllergenFromIngredientAsync_ShouldRemoveRelation_WhenPresentAndOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var allergen = IngredientTestData.CreateAllergen(1, "Laktose", supplier.Id);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Käse");
        ingredient.Allergens.Add(allergen);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        // Act
        await _service.RemoveAllergenFromIngredientAsync(ingredient.Id, allergen.Id, supplier.Id);

        // Assert
        var updated = await _context.Ingredients.Include(i => i.Allergens).FirstAsync(i => i.Id == ingredient.Id);
        Assert.Empty(updated.Allergens);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task RemoveAllergenFromIngredientAsync_ShouldThrowUnauthorizedAccessException_WhenSupplierIsNotOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var owner = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var allergen = IngredientTestData.CreateAllergen(1, "Sellerie", owner.Id);
        var ingredient = GroceryStoreTestData.CreateIngredient(owner.Id, "Brühe");
        ingredient.Allergens.Add(allergen);

        _context.Locations.Add(location);
        _context.Suppliers.Add(owner);
        _context.Ingredients.Add(ingredient);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.RemoveAllergenFromIngredientAsync(ingredient.Id, allergen.Id, Guid.NewGuid()));
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task RemoveAllergenFromIngredientAsync_ShouldThrowInvalidOperationException_WhenRelationDoesNotExist()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var allergen = IngredientTestData.CreateAllergen(1, "Lupine", supplier.Id);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Haferflocken");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RemoveAllergenFromIngredientAsync(ingredient.Id, allergen.Id, supplier.Id));
        Assert.Contains("does not have allergen", ex.Message);
    }

    #endregion
}