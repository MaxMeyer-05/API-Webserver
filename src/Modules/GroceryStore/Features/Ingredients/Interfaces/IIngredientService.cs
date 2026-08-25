namespace GroceryStore.Features.Ingredients.Interfaces;

/// <summary>
/// Defines the contract for a service that manages ingredients in the database.
/// </summary>
public interface IIngredientService
{
    /// <summary>
    /// Retrieves all ingredients from the database.
    /// </summary>
    /// <returns>A collection of ingredient DTOs.</returns>
    Task<IEnumerable<IngredientDto>> GetAllIngredientsAsync();

    /// <summary>
    /// Retrieves a specific ingredient by its ID.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient to retrieve.</param>
    /// <returns>The ingredient DTO if found; otherwise, null.</returns>
    Task<IngredientDto?> GetIngredientByIdAsync(int ingredientId);

    /// <summary>
    /// Creates a new ingredient in the database.
    /// </summary>
    /// <param name="ingredient">The ingredient DTO containing the data for the new ingredient.</param>
    /// <returns>The created ingredient DTO.</returns>
    Task<IngredientDto> CreateIngredientAsync(IngredientCreateDto ingredient);

    /// <summary>
    /// Updates an existing ingredient in the database.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient to update.</param>
    /// <param name="supplierId">The ID of the supplier attempting to update the ingredient.</param>
    /// <param name="ingredient">The ingredient DTO containing the updated data.</param>
    Task UpdateIngredientAsync(int ingredientId, Guid supplierId, IngredientUpdateDto ingredient);

    /// <summary>
    /// Deletes an ingredient from the database by its ID.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient to delete.</param>
    /// <param name="supplierId">The ID of the supplier attempting to delete the ingredient.</param>
    Task DeleteIngredientAsync(int ingredientId, Guid supplierId);


    /// <summary>
    /// Adds an allergen to a specific ingredient in the database.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient to which the allergen will be added.</param>
    /// <param name="allergenId">The ID of the allergen to be added to the ingredient.</param>
    /// <param name="supplierId">The ID of the supplier attempting to add the allergen.</param>
    Task AddAllergenToIngredientAsync(int ingredientId, int allergenId, Guid supplierId);

    /// <summary>
    /// Removes an allergen from a specific ingredient in the database.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient from which the allergen will be removed.</param>
    /// <param name="allergenId">The ID of the allergen to be removed from the ingredient.</param>
    /// <param name="supplierId">The ID of the supplier attempting to remove the allergen.</param>
    Task RemoveAllergenFromIngredientAsync(int ingredientId, int allergenId, Guid supplierId);
}