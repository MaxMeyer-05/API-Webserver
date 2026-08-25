using Moq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Recipes;
using GroceryStore.Features.Recipes.Interfaces;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Recipes;

[Trait("Category", "Service")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Recipes")]
public class RecipeServiceTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<IRecipeMapper> _mapperMock;
    private readonly Mock<ILogger<RecipeService>> _loggerMock;
    private readonly RecipeService _service;

    public RecipeServiceTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;
        _mapperMock = new Mock<IRecipeMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<RecipeService>>();

        _service = new RecipeService(_context, _mapperMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region CreateRecipeAsync Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateRecipeAsync_ShouldPersistEntityAndReturnDto()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = RecipeTestData.CreateRecipeCreateDto(name: "Pizzateig", supplierId: supplier.Id);
        var entity = new Recipe { Name = "Pizzateig", SupplierId = supplier.Id };
        var expectedDto = RecipeTestData.CreateRecipeDto(name: "Pizzateig", supplierId: supplier.Id);

        _mapperMock.Setup(m => m.ToRecipeEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToRecipeDto(It.Is<Recipe>(r => r.Name == "Pizzateig"))).Returns(expectedDto);

        // Act
        var result = await _service.CreateRecipeAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Pizzateig", result.Name);

        var persisted = await _context.Recipes.FirstOrDefaultAsync(r => r.Name == "Pizzateig");
        Assert.NotNull(persisted);
        Assert.NotEqual(0, persisted.Id);

        _mapperMock.Verify(m => m.ToRecipeEntity(createDto), Times.Once);
        _mapperMock.Verify(m => m.ToRecipeDto(entity), Times.Once);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateRecipeAsync_ShouldThrowInvalidOperationException_WhenMappingFails()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = RecipeTestData.CreateRecipeCreateDto(supplierId: supplier.Id);
        var entity = new Recipe { Name = createDto.Name, SupplierId = supplier.Id };

        _mapperMock.Setup(m => m.ToRecipeEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToRecipeDto(entity)).Returns((RecipeDto)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateRecipeAsync(createDto));
        Assert.Contains("could not be loaded", ex.Message);
    }

    #endregion

    #region GetAllRecipesAsync & GetRecipeByIdAsync Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllRecipesAsync_ShouldReturnAllMappedRecipes()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var recipe1 = RecipeTestData.CreateRecipe(supplierId: supplier.Id, name: "Rezept 1");
        var recipe2 = RecipeTestData.CreateRecipe(supplierId: supplier.Id, name: "Rezept 2");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Recipes.AddRange(recipe1, recipe2);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.ToRecipeDto(It.Is<Recipe>(r => r.Name == "Rezept 1")))
            .Returns(RecipeTestData.CreateRecipeDto(name: "Rezept 1", supplierId: supplier.Id));
        _mapperMock.Setup(m => m.ToRecipeDto(It.Is<Recipe>(r => r.Name == "Rezept 2")))
            .Returns(RecipeTestData.CreateRecipeDto(name: "Rezept 2", supplierId: supplier.Id));

        // Act
        var result = await _service.GetAllRecipesAsync();

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, r => r.Name == "Rezept 1");
        Assert.Contains(list, r => r.Name == "Rezept 2");
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetRecipeByIdAsync_ShouldReturnMappedDto_WhenRecipeExists()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var recipe = RecipeTestData.CreateRecipe(supplierId: supplier.Id, name: "Gulasch");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        var expectedDto = RecipeTestData.CreateRecipeDto(name: "Gulasch", supplierId: supplier.Id);
        _mapperMock.Setup(m => m.ToRecipeDto(It.Is<Recipe>(r => r.Id == recipe.Id))).Returns(expectedDto);

        // Act
        var result = await _service.GetRecipeByIdAsync(recipe.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Gulasch", result.Name);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetRecipeByIdAsync_ShouldThrowKeyNotFoundException_WhenRecipeDoesNotExist()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetRecipeByIdAsync(999));
        Assert.Contains("999", ex.Message);
    }

    #endregion

    #region AddCategoryToRecipeAsync & RemoveCategoryFromRecipeAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task AddCategoryToRecipeAsync_ShouldAttachCategory_WhenValidAndOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var recipe = RecipeTestData.CreateRecipe(supplierId: supplier.Id);
        var category = new Category { Name = "Suppen", SupplierId = supplier.Id };

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Recipes.Add(recipe);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        await _service.AddCategoryToRecipeAsync(recipe.Id, category.Id, supplier.Id);

        // Assert
        var updated = await _context.Recipes.Include(r => r.Categories).FirstAsync(r => r.Id == recipe.Id);
        Assert.Single(updated.Categories);
        Assert.Equal("Suppen", updated.Categories.First().Name);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task AddCategoryToRecipeAsync_ShouldThrowUnauthorizedAccessException_WhenSupplierIsNotOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var owner = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var recipe = RecipeTestData.CreateRecipe(supplierId: owner.Id);
        var category = new Category { Name = "Vorspeisen", SupplierId = owner.Id };

        _context.Locations.Add(location);
        _context.Suppliers.Add(owner);
        _context.Recipes.Add(recipe);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.AddCategoryToRecipeAsync(recipe.Id, category.Id, Guid.NewGuid()));
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task RemoveCategoryFromRecipeAsync_ShouldRemoveCategory_WhenAssignedAndOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var category = new Category { Name = "Snacks", SupplierId = supplier.Id };
        var recipe = RecipeTestData.CreateRecipe(supplierId: supplier.Id);
        recipe.Categories.Add(category);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Recipes.Add(recipe);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        await _service.RemoveCategoryFromRecipeAsync(recipe.Id, category.Id, supplier.Id);

        // Assert
        var updated = await _context.Recipes.Include(r => r.Categories).FirstAsync(r => r.Id == recipe.Id);
        Assert.Empty(updated.Categories);
    }

    #endregion

    #region AddIngredientToRecipeAsync & RemoveIngredientFromRecipeAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task AddIngredientToRecipeAsync_ShouldAddRecipeIngredient_WhenValid()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Salz");
        var recipe = RecipeTestData.CreateRecipe(supplierId: supplier.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        // Act
        await _service.AddIngredientToRecipeAsync(recipe.Id, ingredient.Id, supplier.Id);

        // Assert
        var updated = await _context.Recipes.Include(r => r.RecipeIngredients).FirstAsync(r => r.Id == recipe.Id);
        Assert.Single(updated.RecipeIngredients);
        Assert.Equal(ingredient.Id, updated.RecipeIngredients.First().IngredientId);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task RemoveIngredientFromRecipeAsync_ShouldRemoveIngredient_WhenAssigned()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Pfeffer");
        var recipe = RecipeTestData.CreateRecipe(supplierId: supplier.Id);

        var recipeIngredient = new RecipeIngredient { Ingredient = ingredient, Amount = 2m, Recipe = recipe };
        recipe.RecipeIngredients.Add(recipeIngredient);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        // Act
        await _service.RemoveIngredientFromRecipeAsync(recipe.Id, ingredient.Id, supplier.Id);

        // Assert
        var updated = await _context.Recipes.Include(r => r.RecipeIngredients).FirstAsync(r => r.Id == recipe.Id);
        Assert.Empty(updated.RecipeIngredients);
    }

    #endregion

    #region UpdateRecipeAsync & DeleteRecipeAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateRecipeAsync_ShouldUpdate_WhenSupplierIsOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var recipe = RecipeTestData.CreateRecipe(supplierId: supplier.Id, name: "Alt");

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        var updateDto = RecipeTestData.CreateRecipeUpdateDto("Aktualisiert");
        _mapperMock.Setup(m => m.UpdateRecipeEntity(recipe, updateDto))
            .Callback<Recipe, RecipeUpdateDto>((entity, dto) => entity.Name = dto.Name!);

        // Act
        await _service.UpdateRecipeAsync(recipe.Id, supplier.Id, updateDto);

        // Assert
        var updated = await _context.Recipes.FindAsync(recipe.Id);
        Assert.NotNull(updated);
        Assert.Equal("Aktualisiert", updated.Name);
        _mapperMock.Verify(m => m.UpdateRecipeEntity(recipe, updateDto), Times.Once);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteRecipeAsync_ShouldRemoveEntity_WhenSupplierIsOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var recipe = RecipeTestData.CreateRecipe(supplierId: supplier.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteRecipeAsync(recipe.Id, supplier.Id);

        // Assert
        var exists = await _context.Recipes.AnyAsync(r => r.Id == recipe.Id);
        Assert.False(exists);
    }

    #endregion
}