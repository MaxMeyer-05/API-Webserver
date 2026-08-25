using GroceryStore.Database.Entities;

using GroceryStore.Features.Categories;
using GroceryStore.Features.Recipes;

namespace GroceryStore.Tests.Features.Categories;

public static class CategoryTestData
{
    #region Entity Fixtures

    public static Category CreateCategory(
        int id = 0,
        string name = "Frühstück",
        Guid? supplierId = null,
        List<Recipe>? recipes = null) => new()
    {
        Id = id,
        Name = name,
        SupplierId = supplierId ?? Guid.NewGuid(),
        Recipes = recipes ?? []
    };

    #endregion

    #region DTO Fixtures

    public static CategoryDto CreateCategoryDto(
        string name = "Frühstück",
        Guid? supplierId = null,
        IReadOnlyList<RecipeRefDto>? recipes = null) => new(
        Name: name,
        SupplierId: supplierId ?? Guid.NewGuid(),
        Recipes: recipes ?? [new RecipeRefDto("Porridge", Guid.NewGuid(), "Biohof Nord")]);

    public static CategoryCreateDto CreateCategoryCreateDto(
        string name = "Desserts",
        Guid? supplierId = null,
        List<int>? recipeIds = null) => new(
        Name: name,
        SupplierId: supplierId ?? Guid.NewGuid(),
        RecipeIds: recipeIds ?? [1, 2]);

    public static CategoryUpdateDto CreateCategoryUpdateDto(
        string? name = "Snacks & Riegel",
        List<int>? recipeIds = null) => new(
        Name: name,
        RecipeIds: recipeIds ?? [3]);

    #endregion
}