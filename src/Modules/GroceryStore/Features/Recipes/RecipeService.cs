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
	/// <exception cref="InvalidOperationException"></exception>
    public async Task<RecipeDto> CreateRecipeAsync(RecipeCreateDto recipe)
    {
		var categoryIds = recipe.CategoryIds?
			.Distinct()
			.ToList() ?? [];

		var categories = await _dbContext.Categories
			.Where(item => categoryIds.Contains(item.Id))
			.ToListAsync();

		if (categories.Count != categoryIds.Count)
			throw new InvalidOperationException("One or more recipe categories do not exist");

		var ingredientIds = recipe.Ingredients?
			.Select(item => item.IngredientId)
			.Distinct()
			.ToList() ?? [];

		var existingIngredientCount = await _dbContext.Ingredients
			.CountAsync(item => ingredientIds.Contains(item.Id));

		if (existingIngredientCount != ingredientIds.Count)
			throw new InvalidOperationException("One or more recipe ingredients do not exist");

		var recipeEntity = _recipeMapper.ToRecipeEntity(recipe);
		recipeEntity.Categories = categories;
			
		_dbContext.Recipes.Add(recipeEntity);
		await _dbContext.SaveChangesAsync();

		_logger.LogInformation("Created new recipe with Id {RecipeId}", recipeEntity.Id);

		return await GetRecipeByIdAsync(recipeEntity.Id)
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
			.AsSplitQuery()
			.Include(item => item.Supplier)
				.ThenInclude(supplier => supplier.Recipes)
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
			.AsSplitQuery()
			.Include(item => item.Supplier)
				.ThenInclude(supplier => supplier.Recipes)
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
			.Include(item => item.Categories)
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

		var recipeIngredient = await _dbContext.RecipeIngredients
			.FirstOrDefaultAsync(item => item.RecipeId == recipeId && item.IngredientId == ingredientId)
			?? throw new InvalidOperationException($"Recipe with Id {recipeId} does not have ingredient with Id {ingredientId}");
		
		_dbContext.RecipeIngredients.Remove(recipeIngredient);
		await _dbContext.SaveChangesAsync();
		_logger.LogInformation("Removed ingredient with Id {IngredientId} from recipe with Id {RecipeId}", ingredientId, recipeId);
    }

	/// <inheritdoc />
	/// <exception cref="KeyNotFoundException"></exception>
	/// <exception cref="UnauthorizedAccessException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
    public async Task UpdateRecipeAsync(int recipeId, Guid supplierId, RecipeUpdateDto recipe)
    {
		var recipeEntity = await _dbContext.Recipes
			.AsSplitQuery()
			.Include(item => item.Categories)
			.Include(item => item.RecipeIngredients)
			.Where(item => item.Id == recipeId)
			.FirstOrDefaultAsync()
			?? throw new KeyNotFoundException($"Recipe with Id {recipeId} not found");

		if (recipeEntity.SupplierId != supplierId)
			throw new UnauthorizedAccessException($"Not authorized to update recipe with Id {recipeId} as supplier with Id {supplierId}");

		var categories = await GetCategoriesToAddAsync(recipe.CategoryIds);
		await ValidateIngredientsAsync(recipe.Ingredients);

		_recipeMapper.UpdateRecipeEntity(recipeEntity, recipe);
		AddMissingCategories(recipeEntity, categories);
		AddOrUpdateIngredients(recipeEntity, recipe.Ingredients);

		await _dbContext.SaveChangesAsync();

		_logger.LogInformation("Updated recipe with Id {RecipeId}", recipeId);
    }

	/// <summary>
	/// Adds categories to the recipe that are not already present.
	/// </summary>
	/// <param name="recipe">The recipe entity to which categories will be added.</param>
	/// <param name="categories">The categories to be added to the recipe.</param>
	private static void AddMissingCategories(Recipe recipe, IEnumerable<Category> categories)
	{
		foreach (var category in categories)
		{
			if (!recipe.Categories.Any(item => item.Id == category.Id))
				recipe.Categories.Add(category);
		}
	}

	/// <summary>
	/// Adds or updates ingredients in the recipe based on the provided ingredient DTOs.
	/// </summary>
	/// <param name="recipe">The recipe entity to which ingredients will be added or updated.</param>
	/// <param name="ingredients">
	/// The ingredient DTOs containing the ingredient IDs 
	/// and amounts to be added or updated in the recipe.
	/// </param>
	private static void AddOrUpdateIngredients(
		Recipe recipe,
		IEnumerable<RecipeIngredientItemCreateDto>? ingredients)
	{
		foreach (var ingredient in ingredients ?? [])
		{
			var existingRecipeIngredient = recipe.RecipeIngredients
				.FirstOrDefault(item => item.IngredientId == ingredient.IngredientId);

			if (existingRecipeIngredient is not null)
			{
				existingRecipeIngredient.Amount = ingredient.Amount;
				continue;
			}

			recipe.RecipeIngredients.Add(new RecipeIngredient
			{
				IngredientId = ingredient.IngredientId,
				Amount = ingredient.Amount
			});
		}
	}

	/// <summary>
	/// Retrieves the categories to be added to a recipe based on the provided category IDs.
	/// </summary>
	/// <param name="categoryIds">The list of category IDs to be added to the recipe.</param>
	/// <returns>A list of categories corresponding to the provided category IDs.</returns>
	/// <exception cref="InvalidOperationException"></exception>
	private async Task<List<Category>> GetCategoriesToAddAsync(List<int>? categoryIds)
	{
		if (categoryIds is null)
			return [];

		var distinctCategoryIds = categoryIds.Distinct().ToList();
		var categories = await _dbContext.Categories
			.Where(item => distinctCategoryIds.Contains(item.Id))
			.ToListAsync();

		if (categories.Count != distinctCategoryIds.Count)
			throw new InvalidOperationException("One or more recipe categories do not exist");

		return categories;
	}

	/// <summary>
	/// Validates the provided recipe ingredients to ensure that they are unique and exist in the database.
	/// </summary>
	/// <param name="ingredients">The list of recipe ingredient DTOs to be validated.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <exception cref="InvalidOperationException"></exception>
	private async Task ValidateIngredientsAsync(List<RecipeIngredientItemCreateDto>? ingredients)
	{
		if (ingredients is null)
			return;

		var ingredientIds = ingredients.Select(item => item.IngredientId).ToList();

		if (ingredientIds.Distinct().Count() != ingredientIds.Count)
			throw new InvalidOperationException("A recipe ingredient can only be specified once");

		var existingIngredientCount = await _dbContext.Ingredients
			.CountAsync(item => ingredientIds.Contains(item.Id));

		if (existingIngredientCount != ingredientIds.Count)
			throw new InvalidOperationException("One or more recipe ingredients do not exist");
	}
}