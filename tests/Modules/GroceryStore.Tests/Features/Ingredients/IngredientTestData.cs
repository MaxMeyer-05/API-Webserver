using GroceryStore.Database.Entities;

using GroceryStore.Features.Allergens;
using GroceryStore.Features.Ingredients;

namespace GroceryStore.Tests.Features.Ingredients;

public static class IngredientTestData
{
    #region Entity Fixtures

    public static Ingredient CreateIngredient(
        int id = 0,
        string name = "Bio-Milch",
        string unit = "Liter",
        decimal netPrice = 1.29m,
        int stock = 50,
        Guid? supplierId = null,
        decimal? calories = 64m,
        decimal? carbs = 4.8m,
        decimal? protein = 3.4m,
        List<Allergen>? allergens = null,
        Supplier? supplier = null) => new()
    {
        Id = id,
        Name = name,
        Unit = unit,
        NetPrice = netPrice,
        Stock = stock,
        SupplierId = supplierId ?? Guid.NewGuid(),
        Calories = calories,
        Carbohydrates = carbs,
        Protein = protein,
        Allergens = allergens ?? [],
        Supplier = supplier!
    };

    public static Allergen CreateAllergen(
        int id = 1,
        string name = "Laktose",
        Guid? supplierId = null) => new()
    {
        Id = id,
        Name = name,
        SupplierId = supplierId ?? Guid.NewGuid()
    };

    #endregion

    #region DTO Fixtures

    public static IngredientDto CreateIngredientDto(
        int ingredientId = 1,
        string name = "Bio-Milch",
        string unit = "Liter",
        decimal netPrice = 1.29m,
        int stock = 50,
        Guid? supplierId = null,
        string supplierName = "Biohof Nord",
        List<AllergenDto>? allergens = null) => new(
        IngredientId: ingredientId,
        Name: name,
        Unit: unit,
        NetPrice: netPrice,
        Stock: stock,
        SupplierId: supplierId ?? Guid.NewGuid(),
        SupplierName: supplierName,
        Calories: 64m,
        Carbohydrates: 4.8m,
        Protein: 3.4m,
        Allergens: allergens ?? []);

    public static IngredientCreateDto CreateIngredientCreateDto(
        string name = "Hafermilch",
        string unit = "Liter",
        decimal netPrice = 1.99m,
        int stock = 30,
        Guid? supplierId = null,
        List<int>? allergenIds = null) => new(
        Name: name,
        Unit: unit,
        NetPrice: netPrice,
        Stock: stock,
        SupplierId: supplierId ?? Guid.NewGuid(),
        Calories: 45m,
        Carbohydrates: 6.5m,
        Protein: 1.0m,
        AllergenIds: allergenIds ?? []);

    public static IngredientUpdateDto CreateIngredientUpdateDto(
        string? name = "Hafermilch Barista",
        string? unit = "Liter",
        decimal? netPrice = 2.19m,
        int? stock = 40,
        List<int>? allergenIds = null) => new(
        Name: name,
        Unit: unit,
        NetPrice: netPrice,
        Stock: stock,
        Calories: 55m,
        Carbohydrates: 7.0m,
        Protein: 1.2m,
        AllergenIds: allergenIds);

    #endregion
}