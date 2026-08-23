using GroceryStore.Features.Categories;
using GroceryStore.Features.Ingredients;

namespace GroceryStore.Features.Recipes;

/// <summary>
/// Represents a data transfer object (DTO) for a recipe.
/// </summary>
/// <param name="Name">The name of the recipe.</param>
/// <param name="Instructions">The instructions for preparing the recipe.</param>
/// <param name="PreparationTime">The time required to prepare the recipe.</param>
/// <param name="SupplierId">The identifier of the associated supplier.</param>
/// <param name="SupplierName">The name of the supplier associated with the recipe.</param>
/// <param name="Categories">A list of category DTOs associated with the recipe.</param>
/// <param name="Ingredients">A list of recipe ingredient DTOs associated with the recipe.</param>
public record RecipeDto(
    string Name,
    string? Instructions,
    int? PreparationTime,
    Guid SupplierId,
    string SupplierName,
    List<CategoryDto>? Categories,
    List<RecipeIngredientDto>? Ingredients
);

/// <summary>
/// Lightweight recipe reference used inside category DTOs.
/// </summary>
/// <param name="Name">The name of the recipe.</param>
/// <param name="SupplierId">The identifier of the associated supplier.</param>
/// <param name="SupplierName">The name of the supplier associated with the recipe.</param>
public record RecipeRefDto(
    string Name, 
    Guid SupplierId,
    string SupplierName
);

/// <summary>
/// Represents a data transfer object (DTO) for creating a new recipe.
/// </summary>
/// <param name="Name">The name of the recipe.</param>
/// <param name="Instructions">The instructions for preparing the recipe.</param>
/// <param name="PreparationTime">The time required to prepare the recipe.</param>
/// <param name="SupplierId">The identifier of the associated supplier.</param>
/// <param name="CategoryIds">A list of category identifiers associated with the recipe.</param>
/// <param name="Ingredients">A list of recipe ingredient DTOs associated with the recipe.</param>
public record RecipeCreateDto(
    string Name,
    string? Instructions,
    int? PreparationTime,
    Guid SupplierId,
    List<int>? CategoryIds,
    List<RecipeIngredientItemCreateDto>? Ingredients
);

/// <summary>
/// Represents a data transfer object (DTO) for updating an existing recipe.
/// </summary>
/// <param name="Name">The name of the recipe.</param>
/// <param name="Instructions">The instructions for preparing the recipe.</param>
/// <param name="PreparationTime">The time required to prepare the recipe.</param>
/// <param name="CategoryIds">A list of category identifiers associated with the recipe.</param>
/// <param name="Ingredients">A list of recipe ingredient DTOs associated with the recipe.</param>
public record RecipeUpdateDto(
    string? Name,
    string? Instructions,
    int? PreparationTime,
    List<int>? CategoryIds,
    List<RecipeIngredientItemCreateDto>? Ingredients
);

/// <summary>
/// Data transfer object (DTO) for RecipeIngredient entity
/// </summary>
/// <param name="Ingredient">The ingredient associated with the recipe ingredient.</param>
/// <param name="Amount">The amount of the ingredient used in the recipe.</param>
/// <param name="SupplierId">The identifier of the associated supplier.</param>
/// <param name="SupplierName">The name of the supplier associated with the ingredient.</param>
public record RecipeIngredientDto(
    IngredientDto Ingredient,
    decimal Amount,
    Guid SupplierId,
    string SupplierName
);

/// <summary>
/// Data transfer object (DTO) for creating a new RecipeIngredient entity
/// </summary>
/// <param name="RecipeId">The identifier of the recipe to which the ingredient belongs.</param>
/// <param name="IngredientId">The identifier of the ingredient used in the recipe.</param>
/// <param name="Amount">The amount of the ingredient used in the recipe.</param>
public record RecipeIngredientItemCreateDto(
    int RecipeId,
    int IngredientId,
    decimal Amount
);
