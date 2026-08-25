using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Recipes.Interfaces;

namespace GroceryStore.Features.Recipes;

public class RecipeService : IRecipeService
{
	private readonly GroceryStoreDbContext _dbContext;
	private readonly IRecipeMapper _recipeMapper;
	private readonly ILogger<RecipeService> _logger;

	public RecipeService(
		GroceryStoreDbContext dbContext, 
		IRecipeMapper recipeMapper,
		ILogger<RecipeService> logger)
	{
		_dbContext = dbContext;
		_recipeMapper = recipeMapper;
		_logger = logger;
	}

	/// <inheritdoc />
	/// <exception cref="KeyNotFoundException"></exception>
	/// <exception cref="UnauthorizedAccessException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
    public async Task AddCategoryToRecipeAsync(int recipeId, int categoryId, Guid supplierId)
    {
		var recipe = await _dbContext.Recipes
			.Where(item => item.Id == recipeId)
			.FirstOrDefaultAsync()
			?? throw new KeyNotFoundException($"Recipe with Id {recipeId} not found");

		if (recipe.SupplierId != supplierId)
			throw new UnauthorizedAccessException($"Not authorized to add category to recipe with Id {recipeId} as supplier with Id {supplierId}");

		var category = await _dbContext.Categories
			.Where(item => item.Id == categoryId)
			.FirstOrDefaultAsync()
			?? throw new KeyNotFoundException($"Category with Id {categoryId} not found");
		
		if (recipe.Categories.Contains(category))
			throw new InvalidOperationException($"Recipe with Id {recipeId} already has category with Id {categoryId}");

		recipe.Categories.Add(category);
		await _dbContext.SaveChangesAsync();
		_logger.LogInformation("Added category with Id {CategoryId} to recipe with Id {RecipeId}", categoryId, recipeId);
    }

	/// <inheritdoc />
	/// <exception cref="KeyNotFoundException"></exception>
	/// <exception cref="UnauthorizedAccessException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
    public async Task AddIngredientToRecipeAsync(int recipeId, int ingredientId, Guid supplierId)
    {
		var recipe = await _dbContext.Recipes
			.Where(item => item.Id == recipeId)
			.FirstOrDefaultAsync()
			?? throw new KeyNotFoundException($"Recipe with Id {recipeId} not found");

		if (recipe.SupplierId != supplierId)
			throw new UnauthorizedAccessException($"Not authorized to add ingredient to recipe with Id {recipeId} as supplier with Id {supplierId}");

		var ingredient = await _dbContext.RecipeIngredients
			.Where(item => item.IngredientId == ingredientId)
			.FirstOrDefaultAsync()
			?? throw new KeyNotFoundException($"Ingredient with Id {ingredientId} not found");

		if (recipe.RecipeIngredients.Any(ri => ri.IngredientId == ingredientId))
			throw new InvalidOperationException($"Recipe with Id {recipeId} already has ingredient with Id {ingredientId}");

		recipe.RecipeIngredients.Add(new RecipeIngredient
		{
			RecipeId = recipeId,
			IngredientId = ingredientId
		});
		await _dbContext.SaveChangesAsync();
		_logger.LogInformation("Added ingredient with Id {IngredientId} to recipe with Id {RecipeId}", ingredientId, recipeId);
    }

	/// <inheritdoc />
	/// <exception cref="InvalidOperationException"></exception>
    public async Task<RecipeDto> CreateRecipeAsync(RecipeCreateDto recipe)
    {
		var recipeEntity = _recipeMapper.ToRecipeEntity(
			recipe, 
			await GetNextSupplierRecipeCount(recipe.SupplierId));
			
		_dbContext.Recipes.Add(recipeEntity);
		await _dbContext.SaveChangesAsync();

		_logger.LogInformation("Created new recipe with Id {RecipeId}", recipeEntity.Id);

		return _recipeMapper.ToRecipeDto(recipeEntity)
			?? throw new InvalidOperationException($"Created recipe with ID {recipeEntity.Id} could not be loaded");
    }

	/// <inheritdoc />
	/// <exception cref="KeyNotFoundException"></exception>
	/// <exception cref="UnauthorizedAccessException"></exception>
    public async Task DeleteRecipeAsync(int recipeId, Guid supplierId)
    {
		var recipe = await _dbContext.Recipes
			.Where(item => item.Id == recipeId)
			.FirstOrDefaultAsync()
			?? throw new KeyNotFoundException($"Recipe with Id {recipeId} not found or not authorized");

		if (recipe.SupplierId != supplierId)
			throw new UnauthorizedAccessException($"Not authorized to delete recipe with Id {recipeId} as supplier with Id {supplierId}");

		_dbContext.Recipes.Remove(recipe);
		await _dbContext.SaveChangesAsync();
		_logger.LogInformation("Deleted recipe with Id {RecipeId}", recipeId);
    }

	/// <inheritdoc />
    public async Task<IEnumerable<RecipeDto>> GetAllRecipesAsync()
    {
		var recipes = await _dbContext.Recipes
			.AsNoTracking()
			.Include(item => item.Supplier)
			.Include(item => item.RecipeIngredients)
				.ThenInclude(item => item.Ingredient)
					.ThenInclude(item => item.Supplier)
			.Include(item => item.RecipeIngredients)
				.ThenInclude(item => item.Ingredient)
					.ThenInclude(item => item.Allergens)
			.ToListAsync();

		return recipes.Select(item => _recipeMapper.ToRecipeDto(item));
    }

	/// <inheritdoc />
	/// <exception cref="KeyNotFoundException"></exception>
    public async Task<RecipeDto?> GetRecipeByIdAsync(int recipeId)
    {
		var recipe = await _dbContext.Recipes
			.AsNoTracking()
			.Include(item => item.Supplier)
			.Include(item => item.RecipeIngredients)
				.ThenInclude(item => item.Ingredient)
					.ThenInclude(item => item.Supplier)
			.Include(item => item.RecipeIngredients)
				.ThenInclude(item => item.Ingredient)
					.ThenInclude(item => item.Allergens)
			.FirstOrDefaultAsync(item => item.Id == recipeId)
			?? throw new KeyNotFoundException($"Recipe with Id {recipeId} not found");

		return _recipeMapper.ToRecipeDto(recipe);
    }

	/// <inheritdoc />
	/// <exception cref="KeyNotFoundException"></exception>
	/// <exception cref="UnauthorizedAccessException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
    public async Task RemoveCategoryFromRecipeAsync(int recipeId, int categoryId, Guid supplierId)
    {
		var recipe = await _dbContext.Recipes
			.Where(item => item.Id == recipeId)
			.FirstOrDefaultAsync()
			?? throw new KeyNotFoundException($"Recipe with Id {recipeId} not found");

		if (recipe.SupplierId != supplierId)
			throw new UnauthorizedAccessException($"Not authorized to remove category from recipe with Id {recipeId} as supplier with Id {supplierId}");
		
		var category = await _dbContext.Categories
			.Where(item => item.Id == categoryId)
			.FirstOrDefaultAsync()
			?? throw new KeyNotFoundException($"Category with Id {categoryId} not found");

		if (!recipe.Categories.Contains(category))
			throw new InvalidOperationException($"Recipe with Id {recipeId} does not have category with Id {categoryId}");

		recipe.Categories.Remove(category);
		await _dbContext.SaveChangesAsync();
		_logger.LogInformation("Removed category with Id {CategoryId} from recipe with Id {RecipeId}", categoryId, recipeId);
    }

	/// <inheritdoc />
	/// <exception cref="KeyNotFoundException"></exception>
	/// <exception cref="UnauthorizedAccessException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
    public async Task RemoveIngredientFromRecipeAsync(int recipeId, int ingredientId, Guid supplierId)
    {
		var recipe = await _dbContext.Recipes
			.Where(item => item.Id == recipeId)
			.FirstOrDefaultAsync()
			?? throw new KeyNotFoundException($"Recipe with Id {recipeId} not found");

		if (recipe.SupplierId != supplierId)
			throw new UnauthorizedAccessException($"Not authorized to remove ingredient from recipe with Id {recipeId} as supplier with Id {supplierId}");

		var recipeIngredient = recipe.RecipeIngredients
			.FirstOrDefault(ri => ri.IngredientId == ingredientId)
			?? throw new InvalidOperationException($"Recipe with Id {recipeId} does not have ingredient with Id {ingredientId}");
		
		recipe.RecipeIngredients.Remove(recipeIngredient);
		await _dbContext.SaveChangesAsync();
		_logger.LogInformation("Removed ingredient with Id {IngredientId} from recipe with Id {RecipeId}", ingredientId, recipeId);
    }

	/// <inheritdoc />
	/// <exception cref="KeyNotFoundException"></exception>
	/// <exception cref="UnauthorizedAccessException"></exception>
    public async Task UpdateRecipeAsync(int recipeId, Guid supplierId, RecipeUpdateDto recipe)
    {
		var recipeEntity = await _dbContext.Recipes
			.Where(item => item.Id == recipeId)
			.FirstOrDefaultAsync()
			?? throw new KeyNotFoundException($"Recipe with Id {recipeId} not found");

		if (recipeEntity.SupplierId != supplierId)
			throw new UnauthorizedAccessException($"Not authorized to update recipe with Id {recipeId} as supplier with Id {supplierId}");

		_recipeMapper.UpdateRecipeEntity(recipeEntity, recipe);
		await _dbContext.SaveChangesAsync();

		_logger.LogInformation("Updated recipe with Id {RecipeId}", recipeId);
    }

	private async Task<int> GetNextSupplierRecipeCount(Guid supplierId)
	{
		var supplierRecipeCount = await _dbContext.Recipes
			.CountAsync(r => r.SupplierId == supplierId);

		return supplierRecipeCount + 1;
	}
}