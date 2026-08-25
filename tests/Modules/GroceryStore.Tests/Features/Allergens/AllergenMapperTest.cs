using GroceryStore.Features.Allergens;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Allergens;

[Trait("Category", "Unit")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Allergens")]
public class AllergenMapperTest
{
    private readonly AllergenMapper _mapper = new();

    #region ToAllergenDto Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToAllergenDto_ShouldMapAllProperties_WhenIngredientsArePresent()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var ingredient = GroceryStoreTestData.CreateIngredient(supplierId, "Weizenmehl");
        var allergen = AllergenTestData.CreateAllergen(1, "Gluten", supplierId, [ingredient]);

        // Act
        var dto = _mapper.ToAllergenDto(allergen);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("Gluten", dto.Name);
        Assert.Equal(supplierId, dto.SupplierId);
        Assert.NotNull(dto.Ingredients);
        var ingredientRef = Assert.Single(dto.Ingredients);
        Assert.Equal("Weizenmehl", ingredientRef.Name);
        Assert.Equal(supplierId, ingredientRef.SupplierId);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToAllergenDto_ShouldReturnEmptyIngredients_WhenAllergenHasNoIngredients()
    {
        // Arrange
        var allergen = AllergenTestData.CreateAllergen(1, "Laktose", ingredients: []);

        // Act
        var dto = _mapper.ToAllergenDto(allergen);

        // Assert
        Assert.NotNull(dto);
        Assert.NotNull(dto.Ingredients);
        Assert.Empty(dto.Ingredients);
    }

    #endregion

    #region ToAllergenEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToAllergenEntity_ShouldMapCreateDtoToEntityWithIngredientStubs()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var createDto = AllergenTestData.CreateAllergenCreateDto("Erdnüsse", supplierId, [10, 20]);

        // Act
        var entity = _mapper.ToAllergenEntity(createDto);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("Erdnüsse", entity.Name);
        Assert.Equal(supplierId, entity.SupplierId);
        Assert.Equal(2, entity.Ingredients.Count);
        Assert.Contains(entity.Ingredients, i => i.Id == 10);
        Assert.Contains(entity.Ingredients, i => i.Id == 20);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToAllergenEntity_ShouldInitializeEmptyIngredientList_WhenIngredientIdsAreNull()
    {
        // Arrange
        var createDto = new AllergenCreateDto("Soja", Guid.NewGuid(), null);

        // Act
        var entity = _mapper.ToAllergenEntity(createDto);

        // Assert
        Assert.NotNull(entity);
        Assert.NotNull(entity.Ingredients);
        Assert.Empty(entity.Ingredients);
    }

    #endregion

    #region UpdateAllergenEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateAllergenEntity_ShouldUpdateNameAndIngredients_WhenBothProvided()
    {
        // Arrange
        var initialIngredient = GroceryStoreTestData.CreateIngredient(Guid.NewGuid(), "Milch");
        var allergen = AllergenTestData.CreateAllergen(1, "Alt", ingredients: [initialIngredient]);
        var updateDto = new AllergenUpdateDto("Neu", [99]);

        // Act
        _mapper.UpdateAllergenEntity(allergen, updateDto);

        // Assert
        Assert.Equal("Neu", allergen.Name);
        var ingredient = Assert.Single(allergen.Ingredients);
        Assert.Equal(99, ingredient.Id);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateAllergenEntity_ShouldPreserveExistingValues_WhenPropertiesAreNull()
    {
        // Arrange
        var initialIngredient = GroceryStoreTestData.CreateIngredient(Guid.NewGuid(), "Milch");
        var allergen = AllergenTestData.CreateAllergen(1, "Gluten", ingredients: [initialIngredient]);
        var updateDto = new AllergenUpdateDto(null, null);

        // Act
        _mapper.UpdateAllergenEntity(allergen, updateDto);

        // Assert
        Assert.Equal("Gluten", allergen.Name);
        Assert.Single(allergen.Ingredients);
        Assert.Same(initialIngredient, allergen.Ingredients.First());
    }

    #endregion
}