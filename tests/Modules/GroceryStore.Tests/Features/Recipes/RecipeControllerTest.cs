using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Recipes;
using GroceryStore.Features.Recipes.Interfaces;

using GroceryStore.Tests.TestData;
using SharedKernel.Security.Interfaces;

namespace GroceryStore.Tests.Features.Recipes;

[Trait("Category", "Controller")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Recipes")]
public class RecipeControllerTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<IRecipeMapper> _mapperMock;
    private readonly Mock<ILogger<RecipeService>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly RecipeService _service;
    private readonly RecipeController _controller;

    public RecipeControllerTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;

        _mapperMock = new Mock<IRecipeMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<RecipeService>>();
        _currentUserMock = new Mock<ICurrentUser>(MockBehavior.Strict);

        _service = new RecipeService(_context, _mapperMock.Object, _loggerMock.Object);
        _controller = new RecipeController(_service, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetAllRecipes & GetRecipeById Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllRecipes_ShouldReturnOkWithRecipeCollection()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var recipe = RecipeTestData.CreateRecipe(supplierId: supplier.Id);

        _context.Suppliers.Add(supplier);
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        var expectedDto = RecipeTestData.CreateRecipeDto(supplierId: supplier.Id);
        _mapperMock.Setup(m => m.ToRecipeDto(It.IsAny<Recipe>())).Returns(expectedDto);

        // Act
        var result = await _controller.GetAllRecipes();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var list = Assert.IsAssignableFrom<IEnumerable<RecipeDto>>(okResult.Value);
        Assert.Single(list);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllRecipes_ShouldReturnOkWithEmptyCollection_WhenNoRecipesExist()
    {
        // Act
        var result = await _controller.GetAllRecipes();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var recipes = Assert.IsAssignableFrom<IEnumerable<RecipeDto>>(okResult.Value);
        Assert.Empty(recipes);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetRecipeById_ShouldReturnOk_WhenRecipeExists()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var recipe = RecipeTestData.CreateRecipe(supplierId: supplier.Id, name: "Curry");

        _context.Suppliers.Add(supplier);
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        var expectedDto = RecipeTestData.CreateRecipeDto(name: "Curry", supplierId: supplier.Id);
        _mapperMock.Setup(m => m.ToRecipeDto(It.Is<Recipe>(r => r.Id == recipe.Id))).Returns(expectedDto);

        // Act
        var result = await _controller.GetRecipeById(recipe.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedDto, okResult.Value);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetRecipeById_ShouldReturnNotFound_WhenRecipeDoesNotExist()
    {
        // Act
        var result = await _controller.GetRecipeById(404);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region CreateRecipe Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateRecipe_ShouldReturnCreatedAtAction_WhenValid()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = RecipeTestData.CreateRecipeCreateDto(
            "Suppe",
            supplier.Id,
            categoryIds: [],
            ingredients: []);
        var entity = new Recipe { Name = "Suppe", SupplierId = supplier.Id };
        var createdDto = RecipeTestData.CreateRecipeDto(name: "Suppe", supplierId: supplier.Id);

        _mapperMock.Setup(m => m.ToRecipeEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToRecipeDto(It.Is<Recipe>(r => r.Name == "Suppe"))).Returns(createdDto);

        // Act
        var result = await _controller.CreateRecipe(createDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, createdAtResult.StatusCode);
        Assert.Equal(nameof(RecipeController.GetRecipeById), createdAtResult.ActionName);
        Assert.Equal(createdDto.RecipeId, createdAtResult.RouteValues!["recipeId"]);
        Assert.Same(createdDto, createdAtResult.Value);
    }

    #endregion

    #region Category Management Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task RemoveCategoryFromRecipe_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var category = new Category { Name = "Salate", SupplierId = supplier.Id };
        var recipe = RecipeTestData.CreateRecipe(supplierId: supplier.Id);
        recipe.Categories.Add(category);

        _context.Suppliers.Add(supplier);
        _context.Recipes.Add(recipe);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);

        // Act
        var result = await _controller.RemoveCategoryFromRecipe(recipe.Id, category.Id);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        Assert.Empty(recipe.Categories);
    }

    #endregion

    #region UpdateRecipe & DeleteRecipe Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateRecipe_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var recipe = RecipeTestData.CreateRecipe(supplierId: supplier.Id, name: "Baguette");

        _context.Suppliers.Add(supplier);
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        var updateDto = RecipeTestData.CreateRecipeUpdateDto("Knoblauchbaguette");
        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);
        _mapperMock.Setup(m => m.UpdateRecipeEntity(recipe, updateDto))
            .Callback<Recipe, RecipeUpdateDto>((e, dto) => e.Name = dto.Name!);

        // Act
        var result = await _controller.UpdateRecipe(recipe.Id, updateDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        Assert.Equal("Knoblauchbaguette", (await _context.Recipes.FindAsync(recipe.Id))!.Name);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteRecipe_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var recipe = RecipeTestData.CreateRecipe(supplierId: supplier.Id);

        _context.Suppliers.Add(supplier);
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);

        // Act
        var result = await _controller.DeleteRecipe(recipe.Id);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        Assert.Null(await _context.Recipes.FindAsync(recipe.Id));
    }

    #endregion
}