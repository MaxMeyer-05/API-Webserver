using Microsoft.EntityFrameworkCore;

using GroceryStore.Features.Ingredients.Interfaces;

using GroceryStore.Database.DbContexts;

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
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task AddAllergenToIngredientAsync(int ingredientId, int allergenId, Guid supplierId)
    {
        var ingredient = await _dbContext.Ingredients
            .Include(i => i.Allergens)
            .Where(i => i.Id == ingredientId)
            .FirstOrDefaultAsync() 
            ?? throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found");

        if (ingredient.SupplierId != supplierId)
            throw new UnauthorizedAccessException("Only the supplier who owns the ingredient can add allergens.");

        var allergen = await _dbContext.Allergens.FindAsync(allergenId) 
            ?? throw new KeyNotFoundException($"Allergen with ID {allergenId} not found");

        if (!ingredient.Allergens.Contains(allergen))
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
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<IngredientDto> CreateIngredientAsync(IngredientCreateDto ingredient)
    {
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
        var ingredientEntity = await _dbContext.Ingredients.FindAsync(ingredientId) 
            ?? throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found");
        
        if (ingredientEntity.SupplierId != supplierId)
            throw new UnauthorizedAccessException("Only the supplier who owns the ingredient can update it.");

        _ingredientMapper.UpdateIngredientEntity(ingredientEntity, ingredient);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated ingredient with ID {IngredientId}", ingredientId);
    }
}