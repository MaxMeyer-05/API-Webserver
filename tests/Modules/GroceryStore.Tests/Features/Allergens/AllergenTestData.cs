using GroceryStore.Database.Entities;

using GroceryStore.Features.Allergens;
using GroceryStore.Features.Ingredients;

namespace GroceryStore.Tests.Features.Allergens;

public static class AllergenTestData
{
    #region Entity Fixtures

    public static Allergen CreateAllergen(
        int id = 0,
        string name = "Gluten",
        Guid? supplierId = null,
        List<Ingredient>? ingredients = null) => new()
    {
        Id = id,
        Name = name,
        SupplierId = supplierId ?? Guid.NewGuid(),
        Ingredients = ingredients ?? []
    };

    #endregion

    #region DTO Fixtures

    public static AllergenDto CreateAllergenDto(
        string name = "Gluten",
        Guid? supplierId = null,
        IReadOnlyList<IngredientRefDto>? ingredients = null) => new(
        Name: name,
        SupplierId: supplierId ?? Guid.NewGuid(),
        Ingredients: ingredients ?? [new IngredientRefDto("Weizenmehl", Guid.NewGuid())]);

    public static AllergenCreateDto CreateAllergenCreateDto(
        string name = "Erdnüsse",
        Guid? supplierId = null,
        List<int>? ingredientIds = null) => new(
        Name: name,
        SupplierId: supplierId ?? Guid.NewGuid(),
        IngredientIds: ingredientIds ?? [1, 2]);

    public static AllergenUpdateDto CreateAllergenUpdateDto(
        string? name = "Schalenfrüchte",
        List<int>? ingredientIds = null) => new(
        Name: name,
        IngredientIds: ingredientIds ?? [3]);

    #endregion
}