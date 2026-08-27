using Microsoft.EntityFrameworkCore;

using GroceryStore.Features.Ingredients.Interfaces;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

namespace GroceryStore.Features.Ingredients;

/// <summary>
/// Represents the service responsible for managing ingredients in the grocery store application.
/// </summary>
public class IngredientService : IIngredientService
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly IIngredientMapper _ingredientMapper;
    private readonly ILogger<IngredientService> _logger;

    public IngredientService(
        GroceryStoreDbContext dbContext,
        IIngredientMapper ingredientMapper,
        ILogger<IngredientService> logger)
    {
        _dbContext = dbContext;
        _ingredientMapper = ingredientMapper;
        _logger = logger;
    }
    
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<IngredientDto> CreateIngredientAsync(IngredientCreateDto ingredient)
    {
        var supplierExists = await _dbContext.Suppliers
            .AnyAsync(supplier => supplier.Id == ingredient.SupplierId);

        if (!supplierExists)
            throw new InvalidOperationException($"Supplier with ID {ingredient.SupplierId} not found");

        var ingredientEntity = _ingredientMapper.ToIngredientEntity(ingredient);

        _dbContext.Ingredients.Add(ingredientEntity);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new ingredient with ID {IngredientId}", ingredientEntity.Id);

        return _ingredientMapper.ToIngredientDto(ingredientEntity)
            ?? throw new InvalidOperationException("Failed to map the created ingredient entity to DTO");
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task DeleteIngredientAsync(int ingredientId, Guid supplierId)
    {
        var ingredientEntity = await _dbContext.Ingredients
            .Where(i => i.Id == ingredientId)
            .FirstOrDefaultAsync() 
            ?? throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found");

        if (ingredientEntity.SupplierId != supplierId)
            throw new UnauthorizedAccessException("Only the supplier who owns the ingredient can delete it.");

        _dbContext.Ingredients.Remove(ingredientEntity);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted ingredient with ID {IngredientId}", ingredientId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IngredientDto>> GetAllIngredientsAsync()
    {
        var ingredients = await _dbContext.Ingredients
            .Include(i => i.Allergens)
            .Include(i => i.Supplier)
            .ToListAsync();

        _logger.LogDebug("Retrieved all ingredients from the database: {@Ingredients}", ingredients);
        return ingredients.Select(i => _ingredientMapper.ToIngredientDto(i));
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<IngredientDto?> GetIngredientByIdAsync(int ingredientId)
    {
        var ingredient = await _dbContext.Ingredients
            .Include(i => i.Allergens)
            .Include(i => i.Supplier)
            .Where(i => i.Id == ingredientId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found");

        _logger.LogDebug("Retrieved ingredient with ID {IngredientId}: {@Ingredient}", ingredientId, ingredient);
        return ingredient == null ? null : _ingredientMapper.ToIngredientDto(ingredient);
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task RemoveAllergenFromIngredientAsync(int ingredientId, int allergenId, Guid supplierId)
    {
        var ingredient = await _dbContext.Ingredients
            .Include(i => i.Allergens)
            .Where(i => i.Id == ingredientId)
            .FirstOrDefaultAsync() 
            ?? throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found");

        if (ingredient.SupplierId != supplierId)
            throw new UnauthorizedAccessException("Only the supplier who owns the ingredient can remove allergens.");

        var allergen = await _dbContext.Allergens.FindAsync(allergenId) 
            ?? throw new KeyNotFoundException($"Allergen with ID {allergenId} not found");

        var removed = ingredient.Allergens.Remove(allergen);
        if (removed)
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Removed allergen with ID {AllergenId} from ingredient with ID {IngredientId}", allergenId, ingredientId);
        }
        else
        {
            throw new InvalidOperationException($"Ingredient with ID {ingredientId} does not have allergen with ID {allergenId}");
        }
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task UpdateIngredientAsync(int ingredientId, Guid supplierId, IngredientUpdateDto ingredient)
    {
        var ingredientEntity = await _dbContext.Ingredients
            .Include(item => item.Allergens)
            .FirstOrDefaultAsync(item => item.Id == ingredientId)
            ?? throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found");
        
        if (ingredientEntity.SupplierId != supplierId)
            throw new UnauthorizedAccessException("Only the supplier who owns the ingredient can update it.");

        var allergens = await GetAllergensToAddAsync(ingredient.AllergenIds);

        _ingredientMapper.UpdateIngredientEntity(ingredientEntity, ingredient);
        AddMissingAllergens(ingredientEntity, allergens);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated ingredient with ID {IngredientId}", ingredientId);
    }

    /// <summary>
    /// Adds missing allergens to the ingredient's allergen list.
    /// </summary>
    /// <param name="ingredient">The ingredient entity to which allergens will be added.</param>
    /// <param name="allergens">The list of allergens to be added.</param>
    private static void AddMissingAllergens(Ingredient ingredient, IEnumerable<Allergen> allergens)
    {
        foreach (var allergen in allergens)
        {
            if (!ingredient.Allergens.Any(item => item.Id == allergen.Id))
                ingredient.Allergens.Add(allergen);
        }
    }

    /// <summary>
    /// Retrieves the allergens to be added to an ingredient based on the provided allergen IDs.
    /// </summary>
    /// <param name="allergenIds">The list of allergen IDs to be added to the ingredient.</param>
    /// <returns>A list of allergens corresponding to the provided allergen IDs.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<List<Allergen>> GetAllergensToAddAsync(List<int>? allergenIds)
    {
        if (allergenIds is null)
            return [];

        var distinctAllergenIds = allergenIds.Distinct().ToList();
        var allergens = await _dbContext.Allergens
            .Where(item => distinctAllergenIds.Contains(item.Id))
            .ToListAsync();

        if (allergens.Count != distinctAllergenIds.Count)
            throw new InvalidOperationException("One or more ingredient allergens do not exist");

        return allergens;
    }
}