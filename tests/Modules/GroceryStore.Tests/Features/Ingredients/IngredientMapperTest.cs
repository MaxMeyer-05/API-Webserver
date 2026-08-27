using GroceryStore.Features.Ingredients;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Ingredients;

[Trait("Category", "Unit")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Ingredients")]
public class IngredientMapperTest
{
    private readonly IngredientMapper _mapper = new();

    #region ToIngredientDto Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToIngredientDto_ShouldMapAllFields_WhenSupplierAndAllergensArePresent()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var allergen = IngredientTestData.CreateAllergen(1, "Laktose", supplier.Id);
        var ingredient = IngredientTestData.CreateIngredient(
            id: 42,
            name: "Vollmilch",
            unit: "Liter",
            netPrice: 1.19m,
            stock: 25,
            supplierId: supplier.Id,
            calories: 68m,
            carbs: 4.9m,
            protein: 3.3m,
            allergens: [allergen],
            supplier: supplier);

        // Act
        var dto = _mapper.ToIngredientDto(ingredient);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(42, dto.IngredientId);
        Assert.Equal("Vollmilch", dto.Name);
        Assert.Equal("Liter", dto.Unit);
        Assert.Equal(1.19m, dto.NetPrice);
        Assert.Equal(25, dto.Stock);
        Assert.Equal(supplier.Id, dto.SupplierId);
        Assert.Equal(supplier.CompanyName, dto.SupplierName);
        Assert.Equal(68m, dto.Calories);
        Assert.Equal(4.9m, dto.Carbohydrates);
        Assert.Equal(3.3m, dto.Protein);
        Assert.NotNull(dto.Allergens);
        var allergenDto = Assert.Single(dto.Allergens);
        Assert.Equal("Laktose", allergenDto.Name);
        Assert.Equal(supplier.Id, allergenDto.SupplierId);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToIngredientDto_ShouldFallbackToEmptySupplierName_WhenSupplierIsNull()
    {
        // Arrange
        var ingredient = IngredientTestData.CreateIngredient(supplier: null);

        // Act
        var dto = _mapper.ToIngredientDto(ingredient);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(string.Empty, dto.SupplierName);
    }

    #endregion

    #region ToIngredientEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToIngredientEntity_ShouldMapCreateDtoToEntityWithAllergenStubs()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var createDto = IngredientTestData.CreateIngredientCreateDto(
            name: "Mandelmilch",
            unit: "Liter",
            netPrice: 2.49m,
            stock: 15,
            supplierId: supplierId,
            allergenIds: [1, 5]);

        // Act
        var entity = _mapper.ToIngredientEntity(createDto);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("Mandelmilch", entity.Name);
        Assert.Equal("Liter", entity.Unit);
        Assert.Equal(2.49m, entity.NetPrice);
        Assert.Equal(15, entity.Stock);
        Assert.Equal(supplierId, entity.SupplierId);
        Assert.Equal(45m, entity.Calories);
        Assert.Equal(2, entity.Allergens.Count);
        Assert.Contains(entity.Allergens, a => a.Id == 1);
        Assert.Contains(entity.Allergens, a => a.Id == 5);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToIngredientEntity_ShouldInitializeEmptyAllergens_WhenAllergenIdsAreNull()
    {
        // Arrange
        var createDto = new IngredientCreateDto("Wasser", "Liter", 0.50m, 100, Guid.NewGuid(), 0m, 0m, 0m, null);

        // Act
        var entity = _mapper.ToIngredientEntity(createDto);

        // Assert
        Assert.NotNull(entity);
        Assert.NotNull(entity.Allergens);
        Assert.Empty(entity.Allergens);
    }

    #endregion

    #region UpdateIngredientEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateIngredientEntity_ShouldUpdateScalarFields_WhenDtoHasFullValues()
    {
        // Arrange
        var initialAllergen = IngredientTestData.CreateAllergen(1, "Gluten");
        var ingredient = IngredientTestData.CreateIngredient(
            name: "Altes Mehl",
            unit: "g",
            netPrice: 0.99m,
            stock: 10,
            allergens: [initialAllergen]);

        var updateDto = new IngredientUpdateDto(
            Name: "Neues Mehl",
            Unit: "kg",
            NetPrice: 1.49m,
            Stock: 80,
            Calories: 350m,
            Carbohydrates: 70m,
            Protein: 12m,
            AllergenIds: null);

        // Act
        _mapper.UpdateIngredientEntity(ingredient, updateDto);

        // Assert
        Assert.Equal("Neues Mehl", ingredient.Name);
        Assert.Equal("kg", ingredient.Unit);
        Assert.Equal(1.49m, ingredient.NetPrice);
        Assert.Equal(80, ingredient.Stock);
        Assert.Equal(350m, ingredient.Calories);
        Assert.Equal(70m, ingredient.Carbohydrates);
        Assert.Equal(12m, ingredient.Protein);
        var allergen = Assert.Single(ingredient.Allergens);
        Assert.Same(initialAllergen, allergen);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateIngredientEntity_ShouldKeepOriginalValues_WhenDtoHasNullProperties()
    {
        // Arrange
        var initialAllergen = IngredientTestData.CreateAllergen(1, "Gluten");
        var ingredient = IngredientTestData.CreateIngredient(
            name: "Original Name",
            unit: "Stück",
            netPrice: 3.50m,
            stock: 20,
            allergens: [initialAllergen]);

        var updateDto = new IngredientUpdateDto(null, null, null, null, null, null, null, null);

        // Act
        _mapper.UpdateIngredientEntity(ingredient, updateDto);

        // Assert
        Assert.Equal("Original Name", ingredient.Name);
        Assert.Equal("Stück", ingredient.Unit);
        Assert.Equal(3.50m, ingredient.NetPrice);
        Assert.Equal(20, ingredient.Stock);
        Assert.Single(ingredient.Allergens);
        Assert.Same(initialAllergen, ingredient.Allergens.First());
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateIngredientEntity_ShouldApplyZeroValues_WhenProvided()
    {
        // Arrange
        var ingredient = IngredientTestData.CreateIngredient(
            netPrice: 3.50m,
            stock: 20,
            calories: 100m,
            carbs: 25m,
            protein: 10m);
        var updateDto = new IngredientUpdateDto(
            Name: null,
            Unit: null,
            NetPrice: 0m,
            Stock: 0,
            Calories: 0m,
            Carbohydrates: 0m,
            Protein: 0m,
            AllergenIds: null);

        // Act
        _mapper.UpdateIngredientEntity(ingredient, updateDto);

        // Assert
        Assert.Equal(0m, ingredient.NetPrice);
        Assert.Equal(0, ingredient.Stock);
        Assert.Equal(0m, ingredient.Calories);
        Assert.Equal(0m, ingredient.Carbohydrates);
        Assert.Equal(0m, ingredient.Protein);
    }

    #endregion
}