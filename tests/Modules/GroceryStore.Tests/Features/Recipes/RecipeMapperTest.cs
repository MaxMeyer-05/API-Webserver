using Moq;

using GroceryStore.Database.Entities;

using GroceryStore.Features.Categories;
using GroceryStore.Features.Categories.Interfaces;

using GroceryStore.Features.Ingredients;
using GroceryStore.Features.Ingredients.Interfaces;

using GroceryStore.Features.Recipes;
using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Recipes;

[Trait("Category", "Unit")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Recipes")]
public class RecipeMapperTest
{
    private readonly Mock<ICategoryMapper> _categoryMapperMock;
    private readonly Mock<IIngredientMapper> _ingredientMapperMock;
    private readonly RecipeMapper _mapper;

    public RecipeMapperTest()
    {
        _categoryMapperMock = new Mock<ICategoryMapper>(MockBehavior.Strict);
        _ingredientMapperMock = new Mock<IIngredientMapper>(MockBehavior.Strict);
        _mapper = new RecipeMapper(_categoryMapperMock.Object, _ingredientMapperMock.Object);
    }

    #region ToRecipeDto Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToRecipeDto_ShouldMapAllFields_WhenRelationsArePresent()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var category = new Category { Id = 1, Name = "Frühstück", SupplierId = supplier.Id };
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Haferflocken");
        ingredient.Supplier = supplier;

        var recipe = RecipeTestData.CreateRecipe(
            id: 10,
            name: "Porridge",
            supplierId: supplier.Id,
            prepTime: 10,
            instructions: "Kochen und servieren.",
            supplier: supplier);

        var recipeIngredient = RecipeTestData.CreateRecipeIngredient(1, recipe.Id, ingredient.Id, 100m, ingredient);
        recipe.Categories.Add(category);
        recipe.RecipeIngredients.Add(recipeIngredient);
        supplier.Recipes.Add(recipe);

        var categoryDto = new CategoryDto(0, "Frühstück", supplier.Id);
        var ingredientDto = new IngredientDto(
            IngredientId: ingredient.Id,
            SupplierIngredientCount: 0,
            Name: ingredient.Name,
            Unit: ingredient.Unit,
            NetPrice: ingredient.NetPrice,
            Stock: ingredient.Stock,
            SupplierId: supplier.Id,
            SupplierName: supplier.CompanyName,
            Calories: null,
            Carbohydrates: null,
            Protein: null);

        _categoryMapperMock.Setup(m => m.ToCategoryDto(category)).Returns(categoryDto);
        _ingredientMapperMock.Setup(m => m.ToIngredientDto(ingredient)).Returns(ingredientDto);

        // Act
        var dto = _mapper.ToRecipeDto(recipe);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(1, dto.SupplierRecipeCount);
        Assert.Equal("Porridge", dto.Name);
        Assert.Equal("Kochen und servieren.", dto.Instructions);
        Assert.Equal(10, dto.PreparationTime);
        Assert.Equal(supplier.Id, dto.SupplierId);
        Assert.Equal(supplier.CompanyName, dto.SupplierName);

        Assert.NotNull(dto.Categories);
        var cat = Assert.Single(dto.Categories);
        Assert.Equal("Frühstück", cat.Name);

        Assert.NotNull(dto.Ingredients);
        var ing = Assert.Single(dto.Ingredients);
        Assert.Equal(100m, ing.Amount);
        Assert.Equal(supplier.CompanyName, ing.Ingredient.SupplierName);
        Assert.Equal("Haferflocken", ing.Ingredient.Name);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToRecipeDto_ShouldReturnEmptyCollections_WhenCategoriesAndIngredientsAreNull()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var recipe = RecipeTestData.CreateRecipe(supplier: supplier, categories: [], recipeIngredients: []);
        supplier.Recipes.Add(recipe);

        // Act
        var dto = _mapper.ToRecipeDto(recipe);

        // Assert
        Assert.NotNull(dto);
        Assert.Empty(dto.Categories!);
        Assert.Empty(dto.Ingredients!);
    }

    #endregion

    #region ToRecipeEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToRecipeEntity_ShouldMapCreateDtoToEntityWithStubs()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var createDto = RecipeTestData.CreateRecipeCreateDto(
            name: "Waffeln",
            supplierId: supplierId,
            preparationTime: 15,
            instructions: "Backen",
            categoryIds: [1, 2],
            ingredients: [new RecipeIngredientItemCreateDto(5, 250m)]);

        // Act
        var entity = _mapper.ToRecipeEntity(createDto);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("Waffeln", entity.Name);
        Assert.Equal(supplierId, entity.SupplierId);
        Assert.Equal(15, entity.PreparationTime);
        Assert.Equal("Backen", entity.Instructions);

        Assert.Equal(2, entity.Categories.Count);
        Assert.Contains(entity.Categories, c => c.Id == 1);
        Assert.Contains(entity.Categories, c => c.Id == 2);

        var ingredient = Assert.Single(entity.RecipeIngredients);
        Assert.Equal(5, ingredient.IngredientId);
        Assert.Equal(250m, ingredient.Amount);
    }

    #endregion

    #region ToRecipeIngredientDto & ToRecipeIngredientEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToRecipeIngredientDto_ShouldMapRecipeIngredientUsingIngredientMapper()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Zucker");
        ingredient.Supplier = supplier;
        var recipeIngredient = RecipeTestData.CreateRecipeIngredient(1, 10, ingredient.Id, 50m, ingredient);

        var ingredientDto = new IngredientDto(
            IngredientId: ingredient.Id,
            SupplierIngredientCount: 0,
            Name: ingredient.Name,
            Unit: "g",
            NetPrice: 0.89m,
            Stock: 50,
            SupplierId: supplier.Id,
            SupplierName: supplier.CompanyName,
            Calories: null,
            Carbohydrates: null,
            Protein: null);

        _ingredientMapperMock.Setup(m => m.ToIngredientDto(ingredient)).Returns(ingredientDto);

        // Act
        var dto = _mapper.ToRecipeIngredientDto(recipeIngredient);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(50m, dto.Amount);
        Assert.Equal(supplier.Id, dto.Ingredient.SupplierId);
        Assert.Equal(supplier.CompanyName, dto.Ingredient.SupplierName);
        Assert.Equal("Zucker", dto.Ingredient.Name);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToRecipeIngredientEntity_ShouldMapFromItemCreateDto()
    {
        // Arrange
        var itemCreateDto = new RecipeIngredientItemCreateDto(20, 500m);

        // Act
        var entity = _mapper.ToRecipeIngredientEntity(itemCreateDto);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(20, entity.IngredientId);
        Assert.Equal(500m, entity.Amount);
    }

    #endregion

    #region UpdateRecipeEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateRecipeEntity_ShouldUpdateScalarFields_WhenProvidedInDto()
    {
        // Arrange
        var recipe = RecipeTestData.CreateRecipe(name: "Alt", prepTime: 10, instructions: "Alte Anleitung");
        var updateDto = new RecipeUpdateDto(
            Name: "Neu",
            Instructions: "Neue Anleitung",
            PreparationTime: 30,
            CategoryIds: null,
            Ingredients: null);

        // Act
        _mapper.UpdateRecipeEntity(recipe, updateDto);

        // Assert
        Assert.Equal("Neu", recipe.Name);
        Assert.Equal("Neue Anleitung", recipe.Instructions);
        Assert.Equal(30, recipe.PreparationTime);
        Assert.Empty(recipe.Categories);
        Assert.Empty(recipe.RecipeIngredients);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateRecipeEntity_ShouldPreserveExistingValues_WhenDtoHasNulls()
    {
        // Arrange
        var recipe = RecipeTestData.CreateRecipe(name: "Brot", prepTime: 60, instructions: "Backen");
        var updateDto = new RecipeUpdateDto(null, null, null, null, null);

        // Act
        _mapper.UpdateRecipeEntity(recipe, updateDto);

        // Assert
        Assert.Equal("Brot", recipe.Name);
        Assert.Equal("Backen", recipe.Instructions);
        Assert.Equal(60, recipe.PreparationTime);
    }

    #endregion
}