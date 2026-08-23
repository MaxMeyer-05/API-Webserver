using GroceryStore.Features.Recipes.Interfaces;
using GroceryStore.Features.Suppliers.Interfaces;

namespace GroceryStore.Features.Recipes;

/// <summary>
/// Represents a service for managing recipes, 
/// providing business logic and validation for recipe-related operations.
/// </summary>
public class RecipeService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly ILogger<RecipeService> _logger;

    public RecipeService(
        IRecipeRepository recipeRepository, 
        ISupplierRepository supplierRepository,
        ILogger<RecipeService> logger)
    {
        _recipeRepository = recipeRepository;
        _supplierRepository = supplierRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all recipes from the repository.
    /// </summary>
    /// <returns>Returns a collection of all recipes.</returns>
    public async Task<IEnumerable<RecipeDto>> GetAllRecipesAsync()
    {
        _logger.LogDebug("Retrieving all recipes");
        return await _recipeRepository.GetAllRecipesAsync();
    }

    /// <summary>
    /// Retrieves a recipe by its ID from the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to retrieve.</param>
    /// <returns>Returns the recipe if found; otherwise, null.</returns>
    public async Task<RecipeDto?> GetRecipeByIdAsync(int recipeId)
    {
        _logger.LogDebug("Retrieving recipe with ID {RecipeId}", recipeId);
        return await _recipeRepository.GetRecipeByIdAsync(recipeId);
    }

    /// <summary>
    /// Creates a new recipe in the repository.
    /// </summary>
    /// <param name="recipe">The recipe create DTO containing the details of the recipe to create.</param>
    /// <returns>Returns the created recipe DTO.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to create the recipe.</exception>
    public async Task<RecipeDto> CreateRecipeAsync(RecipeCreateDto recipe)
    {
        _logger.LogDebug("Creating new recipe");
        _ = await _supplierRepository.GetSupplierByIdAsync(recipe.SupplierId) 
            ?? throw new UnauthorizedAccessException("Only suppliers are allowed to create recipes.");

        return await _recipeRepository.CreateRecipeAsync(recipe);
    }

    /// <summary>
    /// Updates an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to update.</param>
    /// <param name="supplierId">The ID of the supplier attempting to update the recipe.</param>
    /// <param name="recipe">The recipe update DTO containing the updated details of the recipe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the recipe to update does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to update the recipe.</exception>
    public async Task UpdateRecipeAsync(int recipeId, Guid supplierId, RecipeUpdateDto recipe)
    {
        _logger.LogDebug("Updating recipe with ID {RecipeId}", recipeId);
        var existingRecipe = await _recipeRepository.GetRecipeByIdAsync(recipeId)
            ?? throw new KeyNotFoundException($"Recipe with ID {recipeId} not found.");

        if (existingRecipe.SupplierId != supplierId)
            throw new UnauthorizedAccessException("Only the supplier who created the recipe can update it.");

        await _recipeRepository.UpdateRecipeAsync(recipeId, recipe);
    }

    /// <summary>
    /// Deletes an existing recipe from the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to delete.</param>
    /// <param name="supplierId">The ID of the supplier attempting to delete the recipe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the recipe to delete does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to delete the recipe.</exception>
    public async Task DeleteRecipeAsync(int recipeId, Guid supplierId)
    {
        _logger.LogDebug("Deleting recipe with ID {RecipeId}", recipeId);
        var existingRecipe = await _recipeRepository.GetRecipeByIdAsync(recipeId)
            ?? throw new KeyNotFoundException($"Recipe with ID {recipeId} not found.");

        if (existingRecipe.SupplierId != supplierId)
            throw new UnauthorizedAccessException("Only the supplier who created the recipe can delete it.");

        await _recipeRepository.DeleteRecipeAsync(recipeId);
    }

    /// <summary>
    /// Adds a category to an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to which the category will be added.</param>
    /// <param name="categoryId">The ID of the category to add to the recipe.</param>
    /// <param name="supplierId">The ID of the supplier attempting to add the category.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the recipe to which the category is being added does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to add the category to the recipe.</exception>
    public async Task AddCategoryToRecipeAsync(int recipeId, int categoryId, Guid supplierId)
    {
        _logger.LogDebug("Adding category with ID {CategoryId} to recipe with ID {RecipeId}", categoryId, recipeId);
        var existingRecipe = await _recipeRepository.GetRecipeByIdAsync(recipeId)
            ?? throw new KeyNotFoundException($"Recipe with ID {recipeId} not found.");

        if (existingRecipe.SupplierId != supplierId)
            throw new UnauthorizedAccessException("Only the supplier who created the recipe can add categories to it.");

        await _recipeRepository.AddCategoryToRecipeAsync(recipeId, categoryId);
    }

    /// <summary>
    /// Removes a category from an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe from which the category will be removed.</param>
    /// <param name="categoryId">The ID of the category to remove from the recipe.</param>
    /// <param name="supplierId">The ID of the supplier attempting to remove the category.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the recipe from which the category is being removed does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to remove the category from the recipe.</exception>
    public async Task RemoveCategoryFromRecipeAsync(int recipeId, int categoryId, Guid supplierId)
    {
        _logger.LogDebug("Removing category with ID {CategoryId} from recipe with ID {RecipeId}", categoryId, recipeId);
        var existingRecipe = await _recipeRepository.GetRecipeByIdAsync(recipeId)
            ?? throw new KeyNotFoundException($"Recipe with ID {recipeId} not found.");

        if (existingRecipe.SupplierId != supplierId)
            throw new UnauthorizedAccessException("Only the supplier who created the recipe can remove categories from it.");

        await _recipeRepository.RemoveCategoryFromRecipeAsync(recipeId, categoryId);
    }

    /// <summary>
    /// Adds an ingredient to an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to which the ingredient will be added.</param>
    /// <param name="ingredientId">The ID of the ingredient to add to the recipe.</param>
    /// <param name="supplierId">The ID of the supplier attempting to add the ingredient.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the recipe to which the ingredient is being added does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to add the ingredient to the recipe.</exception>
    public async Task AddIngredientToRecipeAsync(int recipeId, int ingredientId, Guid supplierId)
    {
        _logger.LogDebug("Adding ingredient with ID {IngredientId} to recipe with ID {RecipeId}", ingredientId, recipeId);
        var existingRecipe = await _recipeRepository.GetRecipeByIdAsync(recipeId)
            ?? throw new KeyNotFoundException($"Recipe with ID {recipeId} not found.");

        if (existingRecipe.SupplierId != supplierId)
            throw new UnauthorizedAccessException("Only the supplier who created the recipe can add ingredients to it.");

        await _recipeRepository.AddIngredientToRecipeAsync(recipeId, ingredientId);
    }

    /// <summary>
    /// Removes an ingredient from an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe from which the ingredient will be removed.</param>
    /// <param name="ingredientId">The ID of the ingredient to remove from the recipe.</param>
    /// <param name="supplierId">The ID of the supplier attempting to remove the ingredient.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the recipe from which the ingredient is being removed does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to remove the ingredient from the recipe.</exception>
    public async Task RemoveIngredientFromRecipeAsync(int recipeId, int ingredientId, Guid supplierId)
    {
        _logger.LogDebug("Removing ingredient with ID {IngredientId} from recipe with ID {RecipeId}", ingredientId, recipeId);
        var existingRecipe = await _recipeRepository.GetRecipeByIdAsync(recipeId)
            ?? throw new KeyNotFoundException($"Recipe with ID {recipeId} not found.");

        if (existingRecipe.SupplierId != supplierId)
            throw new UnauthorizedAccessException("Only the supplier who created the recipe can remove ingredients from it.");

        await _recipeRepository.RemoveIngredientFromRecipeAsync(recipeId, ingredientId);
    }
}