using GroceryStore.Features.Allergens;

namespace GroceryStore.Features.Ingredients;

/// <summary>
/// Represents a data transfer object (DTO) for an ingredient.
/// </summary>
/// <param name="Name">The name of the ingredient.</param>
/// <param name="Unit">The unit used to measure the ingredient.</param>
/// <param name="NetPrice">The net price of one unit of the ingredient.</param>
/// <param name="Stock">The current quantity in stock.</param>
/// <param name="SupplierId">The identifier of the supplying company.</param>
/// <param name="Calories">The calorie content of the ingredient.</param>
/// <param name="Carbohydrates">The carbohydrate content of the ingredient.</param>
/// <param name="Protein">The protein content of the ingredient.</param>
/// <param name="Allergens">A list of allergen references associated with the ingredient.</param>
/// <param name="SupplierName">The name of the supplier associated with the ingredient.</param>
public record IngredientDto(
    int IngredientId,
    string Name, 
    string Unit, 
    decimal NetPrice, 
    int Stock, 
    Guid SupplierId, 
    string SupplierName,
    decimal? Calories, 
    decimal? Carbohydrates, 
    decimal? Protein,
    List<AllergenDto>? Allergens = null
);

/// <summary>
/// Lightweight ingredient reference used inside allergen DTOs.
/// </summary>
/// <param name="Name">The name of the ingredient.</param>
/// <param name="SupplierId">The identifier of the supplying company.</param>
public record IngredientRefDto(
    string Name, 
    Guid SupplierId
);

/// <summary>
/// Represents a data transfer object (DTO) for creating a new ingredient.
/// </summary>
/// <param name="Name">The name of the ingredient.</param>
/// <param name="Unit">The unit used to measure the ingredient.</param>
/// <param name="NetPrice">The net price of one unit of the ingredient.</param>
/// <param name="Stock">The current quantity in stock.</param>
/// <param name="SupplierId">The identifier of the supplying company.</param>
/// <param name="Calories">The calorie content of the ingredient.</param>
/// <param name="Carbohydrates">The carbohydrate content of the ingredient.</param>
/// <param name="Protein">The protein content of the ingredient.</param>
/// <param name="AllergenIds">A list of allergen identifiers associated with the ingredient.</param>
public record IngredientCreateDto(
    string Name, 
    string Unit, 
    decimal NetPrice, 
    int Stock, 
    Guid SupplierId, 
    decimal? Calories, 
    decimal? Carbohydrates, 
    decimal? Protein,
    List<int>? AllergenIds
);

/// <summary>
/// Represents a data transfer object (DTO) for updating an existing ingredient.
/// </summary>
/// <param name="Name">The name of the ingredient.</param>
/// <param name="Unit">The unit used to measure the ingredient.</param>
/// <param name="NetPrice">The net price of one unit of the ingredient.</param>
/// <param name="Stock">The current quantity in stock.</param>
/// <param name="Calories">The calorie content of the ingredient.</param>
/// <param name="Carbohydrates">The carbohydrate content of the ingredient.</param>
/// <param name="Protein">The protein content of the ingredient.</param>
/// <param name="AllergenIds">A list of allergen identifiers associated with the ingredient.</param>
public record IngredientUpdateDto(
    string? Name, 
    string? Unit, 
    decimal? NetPrice, 
    int? Stock, 
    decimal? Calories, 
    decimal? Carbohydrates, 
    decimal? Protein,
    List<int>? AllergenIds
);