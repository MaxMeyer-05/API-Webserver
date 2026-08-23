using Microsoft.EntityFrameworkCore;

using GroceryStore.Features.Ingredients.Interfaces;

using GroceryStore.Database.Entities;
using GroceryStore.Database.DbContexts;

namespace GroceryStore.Features.Ingredients;

/// <summary>
/// Represents a repository for managing ingredients in the database.
/// </summary>
public class IngredientRepository : IIngredientRepository
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly IIngredientMapper _ingredientMapper;
    private readonly ILogger<IngredientRepository> _logger;

    public IngredientRepository(
        GroceryStoreDbContext dbContext,
        IIngredientMapper ingredientMapper,
        ILogger<IngredientRepository> logger)
    {
        _dbContext = dbContext;
        _ingredientMapper = ingredientMapper;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task AddAllergenToIngredientAsync(int ingredientId, int allergenId)
    {
        var (hasAllergen, ingredient, allergen) = await HasIngredientAllergenAsync(ingredientId, allergenId);

        if (!hasAllergen)
        {
            ingredient.Allergens.Add(allergen);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Added allergen with ID {AllergenId} to ingredient with ID {IngredientId}", allergenId, ingredientId);
        }
        else
        {
            throw new InvalidOperationException($"Ingredient with ID {ingredientId} already has allergen with ID {allergenId}");
        }
    }

    /// <inheritdoc />
    public async Task<IngredientDto> CreateIngredientAsync(IngredientCreateDto ingredient)
    {
        var ingredientEntity = _ingredientMapper.ToIngredientEntity(ingredient);

        _dbContext.Ingredients.Add(ingredientEntity);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new ingredient with ID {IngredientId}", ingredientEntity.Id);

        var createdIngredient = _ingredientMapper.ToIngredientDto(ingredientEntity);
        return createdIngredient;
    }

    /// <inheritdoc />
    public async Task DeleteIngredientAsync(int ingredientId)
    {
        var ingredientEntity = await _dbContext.Ingredients
            .Where(i => i.Id == ingredientId)
            .ExecuteDeleteAsync();

        if (ingredientEntity == 0)
            throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found");
            
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
    public async Task<IngredientDto?> GetIngredientByIdAsync(int ingredientId)
    {
        var ingredient = await _dbContext.Ingredients
            .Include(i => i.Allergens)
            .Include(i => i.Supplier)
            .Where(i => i.Id == ingredientId)
            .FirstOrDefaultAsync();

        _logger.LogDebug("Retrieved ingredient with ID {IngredientId}: {@Ingredient}", ingredientId, ingredient);
        return ingredient == null ? null : _ingredientMapper.ToIngredientDto(ingredient);
    }

    /// <inheritdoc />
    public async Task RemoveAllergenFromIngredientAsync(int ingredientId, int allergenId)
    {
        var (hasAllergen, ingredient, allergen) = await HasIngredientAllergenAsync(ingredientId, allergenId);
        if (hasAllergen)
        {
            ingredient.Allergens.Remove(allergen);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Removed allergen with ID {AllergenId} from ingredient with ID {IngredientId}", allergenId, ingredientId);
        }
        else
        {
            throw new InvalidOperationException($"Ingredient with ID {ingredientId} does not have allergen with ID {allergenId}");
        }
    }

    /// <inheritdoc />
    public async Task UpdateIngredientAsync(int ingredientId, IngredientUpdateDto ingredient)
    {
        var ingredientEntity = await _dbContext.Ingredients.FindAsync(ingredientId) 
            ?? throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found");
            
        _ingredientMapper.UpdateIngredientEntity(ingredientEntity, ingredient);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated ingredient with ID {IngredientId}", ingredientId);
    }

    /// <summary>
    /// Checks if an ingredient has a specific allergen and retrieves the corresponding entities.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient to check.</param>
    /// <param name="allergenId">The ID of the allergen to check for.</param>
    /// <returns>
    /// A tuple containing a boolean indicating if the ingredient has the allergen, 
    /// the ingredient entity, and the allergen entity.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown if the ingredient or allergen with the specified ID is not found.</exception>
    private async Task<(bool, Ingredient ingredient, Allergen allergen)> HasIngredientAllergenAsync(int ingredientId, int allergenId)
    {
        var ingredient = await _dbContext.Ingredients
            .Include(i => i.Allergens)
            .Where(i => i.Id == ingredientId)
            .FirstOrDefaultAsync() 
            ?? throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found");

        var allergen = await _dbContext.Allergens.FindAsync(allergenId) 
            ?? throw new KeyNotFoundException($"Allergen with ID {allergenId} not found");
            
        return (ingredient.Allergens.Contains(allergen), ingredient, allergen);
    }
}