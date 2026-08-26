using GroceryStore.Database.Entities;

using GroceryStore.Features.Categories;
using GroceryStore.Features.Ingredients;
using GroceryStore.Features.Recipes;

namespace GroceryStore.Tests.Features.Recipes;

public static class RecipeTestData
{
    #region Entity Fixtures

    public static Recipe CreateRecipe(
        int id = 0,
        string name = "Pfannkuchen",
        Guid? supplierId = null,
        int? prepTime = 20,
        string? instructions = "Zutaten vermengen und anbraten.",
        Supplier? supplier = null,
        List<Category>? categories = null,
        List<RecipeIngredient>? recipeIngredients = null) => new()
    {
        Id = id,
        Name = name,
        SupplierId = supplierId ?? Guid.NewGuid(),
        PreparationTime = prepTime,
        Instructions = instructions,
        Supplier = supplier!,
        Categories = categories ?? [],
        RecipeIngredients = recipeIngredients ?? []
    };

    public static RecipeIngredient CreateRecipeIngredient(
        int id = 0,
        int recipeId = 0,
        int ingredientId = 1,
        decimal amount = 200m,
        Ingredient? ingredient = null) => new()
    {
        Id = id,
        RecipeId = recipeId,
        IngredientId = ingredientId,
        Amount = amount,
        Ingredient = ingredient!
    };

    #endregion

    #region DTO Fixtures

    public static RecipeDto CreateRecipeDto(
        int supplierRecipeCount = 1,
        string name = "Pfannkuchen",
        string? instructions = "Zutaten vermengen und anbraten.",
        int? preparationTime = 20,
        Guid? supplierId = null,
        string supplierName = "Biohof Nord",
        List<CategoryDto>? categories = null,
        List<RecipeIngredientDto>? ingredients = null) => new(
            RecipeId: 0,
            SupplierRecipeCount: supplierRecipeCount,
            Name: name,
            Instructions: instructions,
            PreparationTime: preparationTime,
            SupplierId: supplierId ?? Guid.NewGuid(),
            SupplierName: supplierName,
            Categories: categories ?? [],
            Ingredients: ingredients ?? []);

    public static RecipeCreateDto CreateRecipeCreateDto(
        string name = "Waffeln",
        Guid? supplierId = null,
        int? preparationTime = 15,
        string? instructions = "Im Waffeleisen backen.",
        List<int>? categoryIds = null,
        List<RecipeIngredientItemCreateDto>? ingredients = null) => new(
        Name: name,
        Instructions: instructions,
        PreparationTime: preparationTime,
        SupplierId: supplierId ?? Guid.NewGuid(),
        CategoryIds: categoryIds ?? [1],
        Ingredients: ingredients ?? [new RecipeIngredientItemCreateDto(0, 1, 150m)]);

    public static RecipeUpdateDto CreateRecipeUpdateDto(
        string? name = "Kaiserschmarrn",
        string? instructions = "In Stücke reißen und karamellisieren.",
        int? preparationTime = 25,
        List<int>? categoryIds = null,
        List<RecipeIngredientItemCreateDto>? ingredients = null) => new(
        Name: name,
        Instructions: instructions,
        PreparationTime: preparationTime,
        CategoryIds: categoryIds,
        Ingredients: ingredients);

    public static RecipeIngredientDto CreateRecipeIngredientDto(
        int ingredientId = 1,
        string ingredientName = "Mehl",
        decimal amount = 250m,
        Guid? supplierId = null,
        string supplierName = "Biohof Nord") => new(
        Ingredient: new IngredientDto(
            IngredientId: ingredientId,
            Name: ingredientName,
            Unit: "g",
            NetPrice: 0.99m,
            SupplierIngredientCount: 0,
            Stock: 100,
            SupplierId: supplierId ?? Guid.NewGuid(),
            SupplierName: supplierName,
            Calories: 360m,
            Carbohydrates: 72m,
            Protein: 10m),
        Amount: amount,
        SupplierId: supplierId ?? Guid.NewGuid(),
        SupplierName: supplierName);

    #endregion
}