namespace GroceryStore.Features.Recipes.Interfaces;

/// <summary>
/// Defines the contract for a service that manages recipes in the database.
/// </summary>
public interface IRecipeService
{
	/// <summary>
	/// Retrieves all recipes from the database.
	/// </summary>
	/// <returns>A collection of recipe DTOs.</returns>
	Task<IEnumerable<RecipeDto>> GetAllRecipesAsync();

	/// <summary>
	/// Retrieves a specific recipe by its ID from the database.
	/// </summary>
	/// <param name="recipeId">The unique identifier of the recipe.</param>
	/// <returns>The recipe DTO if found; otherwise, null.</returns>
	Task<RecipeDto?> GetRecipeByIdAsync(int recipeId);

	/// <summary>
	/// Creates a new recipe in the database.
	/// </summary>
	/// <param name="recipe">The recipe creation DTO.</param>
	/// <returns>The created recipe DTO.</returns>
	Task<RecipeDto> CreateRecipeAsync(RecipeCreateDto recipe);

	/// <summary>
	/// Adds a category to a specific recipe in the database.
	/// </summary>
	/// <param name="recipeId">The unique identifier of the recipe.</param>
	/// <param name="categoryId">The unique identifier of the category.</param>
	/// <param name="supplierId">The unique identifier of the supplier attempting to add the category to the recipe.</param>
	Task AddCategoryToRecipeAsync(int recipeId, int categoryId, Guid supplierId);

	/// <summary>
	/// Adds an ingredient to a specific recipe in the database.
	/// </summary>
	/// <param name="recipeId">The unique identifier of the recipe.</param>
	/// <param name="ingredientId">The unique identifier of the ingredient.</param>
	/// <param name="supplierId">The unique identifier of the supplier attempting to add the ingredient to the recipe.</param>
	Task AddIngredientToRecipeAsync(int recipeId, int ingredientId, Guid supplierId);

	/// <summary>
	/// Updates an existing recipe in the database.
	/// </summary>
	/// <param name="recipeId">The unique identifier of the recipe.</param>
	/// <param name="supplierId">The unique identifier of the supplier attempting to update the recipe.</param>
	/// <param name="recipe">The recipe update DTO.</param>
	Task UpdateRecipeAsync(int recipeId, Guid supplierId, RecipeUpdateDto recipe);

	/// <summary>
	/// Deletes a specific recipe from the database.
	/// </summary>
	/// <param name="recipeId">The unique identifier of the recipe.</param>
	/// <param name="supplierId">The unique identifier of the supplier attempting to delete the recipe.</param>
	Task DeleteRecipeAsync(int recipeId, Guid supplierId);

	/// <summary>
	/// Removes a category from a specific recipe in the database.
	/// </summary>
	/// <param name="recipeId">The unique identifier of the recipe.</param>
	/// <param name="categoryId">The unique identifier of the category.</param>
	/// <param name="supplierId">The unique identifier of the supplier attempting to remove the category from the recipe.</param>
	Task RemoveCategoryFromRecipeAsync(int recipeId, int categoryId, Guid supplierId);

	/// <summary>
	/// Removes an ingredient from a specific recipe in the database.
	/// </summary>
	/// <param name="recipeId">The unique identifier of the recipe.</param>
	/// <param name="ingredientId">The unique identifier of the ingredient.</param>
	/// <param name="supplierId">The unique identifier of the supplier attempting to remove the ingredient from the recipe.</param>
	Task RemoveIngredientFromRecipeAsync(int recipeId, int ingredientId, Guid supplierId);
}