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
        var supplier = GroceryStoreTestData.CreateSupplier();
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
        var supplier = GroceryStoreTestData.CreateSupplier();
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

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateIngredientAsync_ShouldThrowInvalidOperationException_WhenSupplierDoesNotExist()
    {
        // Arrange
        var createDto = IngredientTestData.CreateIngredientCreateDto(supplierId: Guid.NewGuid());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateIngredientAsync(createDto));
        Assert.Contains(createDto.SupplierId.ToString(), ex.Message);
        _mapperMock.Verify(m => m.ToIngredientEntity(It.IsAny<IngredientCreateDto>()), Times.Never);
    }

    #endregion

    #region GetAllIngredientsAsync & GetIngredientByIdAsync Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllIngredientsAsync_ShouldReturnAllMappedIngredients()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var ingredient1 = GroceryStoreTestData.CreateIngredient(supplier.Id, "Apfel");
        var ingredient2 = GroceryStoreTestData.CreateIngredient(supplier.Id, "Birne");

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
        _mapperMock.Verify(m => m.ToIngredientDto(ingredient1), Times.Once);
        _mapperMock.Verify(m => m.ToIngredientDto(ingredient2), Times.Once);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllIngredientsAsync_ShouldReturnEmptyCollection_WhenNoIngredientsExist()
    {
        // Act
        var result = await _service.GetAllIngredientsAsync();

        // Assert
        Assert.Empty(result);
        _mapperMock.Verify(m => m.ToIngredientDto(It.IsAny<Ingredient>()), Times.Never);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetIngredientByIdAsync_ShouldReturnMappedDto_WhenIngredientExists()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Zucker");

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
        _mapperMock.Verify(m => m.ToIngredientDto(ingredient), Times.Once);
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
        var supplier = GroceryStoreTestData.CreateSupplier();
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Mehl");

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
    public async Task UpdateIngredientAsync_ShouldAddAllergensWithoutRemovingExistingOnes()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var existingAllergen = IngredientTestData.CreateAllergen(1, "Gluten", supplier.Id);
        var additionalAllergen = IngredientTestData.CreateAllergen(2, "Soja", supplier.Id);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Brot");
        ingredient.Allergens.Add(existingAllergen);

        _context.Suppliers.Add(supplier);
        _context.Allergens.AddRange(existingAllergen, additionalAllergen);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        var updateDto = new IngredientUpdateDto(
            Name: null,
            Unit: null,
            NetPrice: null,
            Stock: null,
            Calories: null,
            Carbohydrates: null,
            Protein: null,
            AllergenIds: [additionalAllergen.Id]);
        _mapperMock.Setup(m => m.UpdateIngredientEntity(ingredient, updateDto));

        // Act
        await _service.UpdateIngredientAsync(ingredient.Id, supplier.Id, updateDto);

        // Assert
        var updated = await _context.Ingredients
            .Include(item => item.Allergens)
            .SingleAsync(item => item.Id == ingredient.Id);

        Assert.Contains(updated.Allergens, item => item.Id == existingAllergen.Id);
        Assert.Contains(updated.Allergens, item => item.Id == additionalAllergen.Id);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateIngredientAsync_ShouldThrowUnauthorizedAccessException_WhenSupplierIsNotOwner()
    {
        // Arrange
        var owner = GroceryStoreTestData.CreateSupplier();
        var strangerId = Guid.NewGuid();
        var ingredient = GroceryStoreTestData.CreateIngredient(owner.Id, "Salz");

        _context.Suppliers.Add(owner);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        var updateDto = IngredientTestData.CreateIngredientUpdateDto("Meersalz");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.UpdateIngredientAsync(ingredient.Id, strangerId, updateDto));
        Assert.Equal("Salz", (await _context.Ingredients.FindAsync(ingredient.Id))!.Name);
        _mapperMock.Verify(m => m.UpdateIngredientEntity(It.IsAny<Ingredient>(), It.IsAny<IngredientUpdateDto>()), Times.Never);
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

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateIngredientAsync_ShouldThrowInvalidOperationException_WhenAnAllergenDoesNotExist()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Mehl");
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        var updateDto = new IngredientUpdateDto(null, null, null, null, null, null, null, [404]);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateIngredientAsync(ingredient.Id, supplier.Id, updateDto));
        Assert.Equal("One or more ingredient allergens do not exist", ex.Message);
        _mapperMock.Verify(m => m.UpdateIngredientEntity(It.IsAny<Ingredient>(), It.IsAny<IngredientUpdateDto>()), Times.Never);
    }

    #endregion

    #region DeleteIngredientAsync Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteIngredientAsync_ShouldRemoveEntity_WhenSupplierIsOwner()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Pfeffer");

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
        var owner = GroceryStoreTestData.CreateSupplier();
        var ingredient = GroceryStoreTestData.CreateIngredient(owner.Id, "Oregano");

        _context.Suppliers.Add(owner);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.DeleteIngredientAsync(ingredient.Id, Guid.NewGuid()));
        Assert.NotNull(await _context.Ingredients.FindAsync(ingredient.Id));
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

    #region RemoveAllergenFromIngredientAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task RemoveAllergenFromIngredientAsync_ShouldRemoveRelation_WhenPresentAndOwner()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var allergen = IngredientTestData.CreateAllergen(1, "Laktose", supplier.Id);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Käse");
        ingredient.Allergens.Add(allergen);

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
        var owner = GroceryStoreTestData.CreateSupplier();
        var allergen = IngredientTestData.CreateAllergen(1, "Sellerie", owner.Id);
        var ingredient = GroceryStoreTestData.CreateIngredient(owner.Id, "Brühe");
        ingredient.Allergens.Add(allergen);

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
        var supplier = GroceryStoreTestData.CreateSupplier();
        var allergen = IngredientTestData.CreateAllergen(1, "Lupine", supplier.Id);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Haferflocken");

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