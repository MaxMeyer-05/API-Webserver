using GroceryStore.Features.Ingredients.Interfaces;
using GroceryStore.Features.Suppliers.Interfaces;

namespace GroceryStore.Features.Ingredients;

/// <summary>
/// Represents a service for managing ingredients, 
/// providing business logic and validation for ingredient-related operations.
/// </summary>
public class IngredientService
{
    private readonly IIngredientRepository _ingredientRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly ILogger<IngredientService> _logger;

    public IngredientService(
        IIngredientRepository ingredientRepository, 
        ISupplierRepository supplierRepository, 
        ILogger<IngredientService> logger)
    {
        _ingredientRepository = ingredientRepository;
        _supplierRepository = supplierRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all ingredients from the repository.
    /// </summary>
    /// <returns>Returns a collection of all ingredients.</returns>
    public async Task<IEnumerable<IngredientDto>> GetAllIngredientsAsync()
    {
        _logger.LogDebug("Retrieving all ingredients");
        return await _ingredientRepository.GetAllIngredientsAsync();
    }

    /// <summary>
    /// Retrieves a specific ingredient by its ID.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient to retrieve.</param>
    /// <returns>Returns the ingredient DTO if found; otherwise, null.</returns>
    public async Task<IngredientDto?> GetIngredientByIdAsync(int ingredientId)
    {
        _logger.LogDebug("Retrieving ingredient with ID {IngredientId}", ingredientId);
        return await _ingredientRepository.GetIngredientByIdAsync(ingredientId);
    }

    /// <summary>
    /// Creates a new ingredient in the repository.
    /// </summary>
    /// <param name="ingredient">The ingredient create DTO containing the details of the ingredient to create.</param>
    /// <returns>Returns the created ingredient DTO.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to create the ingredient.</exception>
    public async Task<IngredientDto> CreateIngredientAsync(IngredientCreateDto ingredient)
    {
        _logger.LogDebug("Creating new ingredient");
        var supplier = await _supplierRepository.GetSupplierByIdAsync(ingredient.SupplierId);
        if (supplier == null)
        {
            _logger.LogDebug("Supplier with ID {SupplierId} not found", ingredient.SupplierId);
            throw new UnauthorizedAccessException("Only suppliers are allowed to create ingredients.");
        }

        return await _ingredientRepository.CreateIngredientAsync(ingredient);
    }

    /// <summary>
    /// Updates an existing ingredient in the repository.
    /// </summary>
    /// <param name="supplierId">The ID of the supplier attempting to update the ingredient.</param>
    /// <param name="ingredientId">The ID of the ingredient to update.</param>
    /// <param name="ingredient">The ingredient update DTO containing the updated details.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the ingredient to update does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to update the ingredient.</exception>
    /// <remarks>Only the supplier who owns the ingredient can update it.</remarks>
    public async Task UpdateIngredientAsync(Guid supplierId, int ingredientId, IngredientUpdateDto ingredient)
    {
        _logger.LogDebug("Updating ingredient with ID {IngredientId}", ingredientId);
        var existingIngredient = await _ingredientRepository.GetIngredientByIdAsync(ingredientId);
        if (existingIngredient == null)
        {
            _logger.LogDebug("Ingredient with ID {IngredientId} not found", ingredientId);
            throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found.");
        }

        if (existingIngredient.SupplierId != supplierId)
        {
            _logger.LogDebug("Supplier with ID {SupplierId} is not authorized to update ingredient with ID {IngredientId}", supplierId, ingredientId);
            throw new UnauthorizedAccessException("Only the supplier who owns the ingredient can update it.");
        }

        await _ingredientRepository.UpdateIngredientAsync(ingredientId, ingredient);
    }

    /// <summary>
    /// Deletes an existing ingredient from the repository.
    /// </summary>
    /// <param name="supplierId">The ID of the supplier attempting to delete the ingredient.</param>
    /// <param name="ingredientId">The ID of the ingredient to delete.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the ingredient to delete does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to delete the ingredient.</exception>
    /// <remarks>Only the supplier who owns the ingredient can delete it.</remarks>
    public async Task DeleteIngredientAsync(Guid supplierId, int ingredientId)
    {
        _logger.LogDebug("Deleting ingredient with ID {IngredientId}", ingredientId);
        var existingIngredient = await _ingredientRepository.GetIngredientByIdAsync(ingredientId);
        if (existingIngredient == null)
        {
            _logger.LogDebug("Ingredient with ID {IngredientId} not found", ingredientId);
            throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found.");
        }

        if (existingIngredient.SupplierId != supplierId)
        {
            _logger.LogDebug("Supplier with ID {SupplierId} is not authorized to delete ingredient with ID {IngredientId}", supplierId, ingredientId);
            throw new UnauthorizedAccessException("Only the supplier who owns the ingredient can delete it.");
        }

        await _ingredientRepository.DeleteIngredientAsync(ingredientId);
    }

    /// <summary>
    /// Adds an allergen to an existing ingredient in the repository.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient to which the allergen will be added.</param>
    /// <param name="allergenId">The ID of the allergen to add to the ingredient.</param>
    /// <param name="supplierId">The ID of the supplier attempting to add the allergen.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the ingredient does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to add the allergen.</exception>
    /// <remarks>Only the supplier who owns the ingredient can add allergens.</remarks>
    public async Task AddAllergenToIngredientAsync(int ingredientId, int allergenId, Guid supplierId)
    {
        _logger.LogDebug("Adding allergen with ID {AllergenId} to ingredient with ID {IngredientId}", allergenId, ingredientId);
        var existingIngredient = await _ingredientRepository.GetIngredientByIdAsync(ingredientId);
        if (existingIngredient == null)
        {
            _logger.LogDebug("Ingredient with ID {IngredientId} not found", ingredientId);
            throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found.");
        }

        if (existingIngredient.SupplierId != supplierId)
        {
            _logger.LogDebug("Supplier with ID {SupplierId} is not authorized to add allergen to ingredient with ID {IngredientId}", supplierId, ingredientId);
            throw new UnauthorizedAccessException("Only the supplier who owns the ingredient can add allergens.");
        }

        await _ingredientRepository.AddAllergenToIngredientAsync(ingredientId, allergenId);
    }

    /// <summary>
    /// Removes an allergen from an existing ingredient in the repository.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient from which the allergen will be removed.</param>
    /// <param name="allergenId">The ID of the allergen to remove from the ingredient.</param>
    /// <param name="supplierId">The ID of the supplier attempting to remove the allergen.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the ingredient does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to remove the allergen.</exception>
    /// <remarks>Only the supplier who owns the ingredient can remove allergens.</remarks>
    public async Task RemoveAllergenFromIngredientAsync(int ingredientId, int allergenId, Guid supplierId)
    {
        _logger.LogDebug("Removing allergen with ID {AllergenId} from ingredient with ID {IngredientId}", allergenId, ingredientId);
        var existingIngredient = await _ingredientRepository.GetIngredientByIdAsync(ingredientId);
        if (existingIngredient == null)
        {
            _logger.LogDebug("Ingredient with ID {IngredientId} not found", ingredientId);
            throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found.");
        }

        if (existingIngredient.SupplierId != supplierId)
        {
            _logger.LogDebug("Supplier with ID {SupplierId} is not authorized to remove allergen from ingredient with ID {IngredientId}", supplierId, ingredientId);
            throw new UnauthorizedAccessException("Only the supplier who owns the ingredient can remove allergens.");
        }

        await _ingredientRepository.RemoveAllergenFromIngredientAsync(ingredientId, allergenId);
    }
}