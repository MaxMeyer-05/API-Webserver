using GroceryStore.Features.Categories;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Categories;

[Trait("Category", "Unit")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Categories")]
public class CategoryMapperTest
{
    private readonly CategoryMapper _mapper = new();

    #region ToCategoryDto Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToCategoryDto_ShouldMapAllProperties_WhenRecipesArePresent()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var recipe = GroceryStoreTestData.CreateRecipe(supplier.Id, "Pfannkuchen");
        recipe.Supplier = supplier;

        var category = CategoryTestData.CreateCategory(1, "Frühstück", supplier.Id, [recipe]);

        // Act
        var dto = _mapper.ToCategoryDto(category);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(1, dto.CategoryId);
        Assert.Equal("Frühstück", dto.Name);
        Assert.Equal(supplier.Id, dto.SupplierId);
        Assert.NotNull(dto.Recipes);
        var recipeRef = Assert.Single(dto.Recipes);
        Assert.Equal("Pfannkuchen", recipeRef.Name);
        Assert.Equal(supplier.Id, recipeRef.SupplierId);
        Assert.Equal(supplier.CompanyName, recipeRef.SupplierName);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToCategoryDto_ShouldReturnEmptyRecipes_WhenCategoryHasNoRecipes()
    {
        // Arrange
        var category = CategoryTestData.CreateCategory(1, "Snacks", recipes: []);

        // Act
        var dto = _mapper.ToCategoryDto(category);

        // Assert
        Assert.NotNull(dto);
        Assert.NotNull(dto.Recipes);
        Assert.Empty(dto.Recipes);
    }

    #endregion

    #region ToCategoryEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToCategoryEntity_ShouldMapCreateDtoToEntityWithRecipeStubs()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var createDto = CategoryTestData.CreateCategoryCreateDto("Desserts", supplierId, [10, 20]);

        // Act
        var entity = _mapper.ToCategoryEntity(createDto);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("Desserts", entity.Name);
        Assert.Equal(supplierId, entity.SupplierId);
        Assert.Equal(2, entity.Recipes.Count);
        Assert.Contains(entity.Recipes, r => r.Id == 10);
        Assert.Contains(entity.Recipes, r => r.Id == 20);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToCategoryEntity_ShouldInitializeEmptyRecipeList_WhenRecipeIdsAreNull()
    {
        // Arrange
        var createDto = new CategoryCreateDto("Getränke", Guid.NewGuid(), null);

        // Act
        var entity = _mapper.ToCategoryEntity(createDto);

        // Assert
        Assert.NotNull(entity);
        Assert.NotNull(entity.Recipes);
        Assert.Empty(entity.Recipes);
    }

    #endregion

    #region UpdateCategoryEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateCategoryEntity_ShouldUpdateNameAndRecipes_WhenBothProvided()
    {
        // Arrange
        var initialRecipe = GroceryStoreTestData.CreateRecipe(Guid.NewGuid(), "Müsli");
        var category = CategoryTestData.CreateCategory(1, "Alt", recipes: [initialRecipe]);
        var updateDto = new CategoryUpdateDto("Neu", [99]);

        // Act
        _mapper.UpdateCategoryEntity(category, updateDto);

        // Assert
        Assert.Equal("Neu", category.Name);
        var recipe = Assert.Single(category.Recipes);
        Assert.Equal(99, recipe.Id);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateCategoryEntity_ShouldPreserveExistingValues_WhenPropertiesAreNull()
    {
        // Arrange
        var initialRecipe = GroceryStoreTestData.CreateRecipe(Guid.NewGuid(), "Müsli");
        var category = CategoryTestData.CreateCategory(1, "Frühstück", recipes: [initialRecipe]);
        var updateDto = new CategoryUpdateDto(null, null);

        // Act
        _mapper.UpdateCategoryEntity(category, updateDto);

        // Assert
        Assert.Equal("Frühstück", category.Name);
        Assert.Single(category.Recipes);
        Assert.Same(initialRecipe, category.Recipes.First());
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateCategoryEntity_ShouldClearRecipes_WhenEmptyListIsProvided()
    {
        // Arrange
        var category = CategoryTestData.CreateCategory(
            1,
            "Frühstück",
            recipes: [
                GroceryStoreTestData.CreateRecipe(Guid.NewGuid(), "Müsli"),
                GroceryStoreTestData.CreateRecipe(Guid.NewGuid(), "Porridge")
            ]);
        var updateDto = new CategoryUpdateDto(null, []);

        // Act
        _mapper.UpdateCategoryEntity(category, updateDto);

        // Assert
        Assert.Equal("Frühstück", category.Name);
        Assert.Empty(category.Recipes);
    }

    #endregion
}