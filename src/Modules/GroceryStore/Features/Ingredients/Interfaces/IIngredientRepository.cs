namespace GroceryStore.Features.Ingredients.Interfaces;

/// <summary>
/// Defines the contract for a repository that manages ingredients in the database.
/// </summary>
public interface IIngredientRepository
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
    /// <param name="ingredient">The ingredient DTO containing the updated data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateIngredientAsync(int ingredientId, IngredientUpdateDto ingredient);

    /// <summary>
    /// Deletes an ingredient from the database by its ID.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteIngredientAsync(int ingredientId);


    /// <summary>
    /// Adds an allergen to a specific ingredient in the database.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient to which the allergen will be added.</param>
    /// <param name="allergenId">The ID of the allergen to be added to the ingredient.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAllergenToIngredientAsync(int ingredientId, int allergenId);

    /// <summary>
    /// Removes an allergen from a specific ingredient in the database.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient from which the allergen will be removed.</param>
    /// <param name="allergenId">The ID of the allergen to be removed from the ingredient.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAllergenFromIngredientAsync(int ingredientId, int allergenId);
}