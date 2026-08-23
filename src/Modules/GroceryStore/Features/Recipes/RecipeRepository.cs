using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;
using GroceryStore.Features.Recipes.Interfaces;

namespace GroceryStore.Features.Recipes;

public class RecipeRepository : IRecipeRepository
{
	private readonly GroceryStoreDbContext _dbContext;
	private readonly IRecipeMapper _recipeMapper;

	public RecipeRepository(GroceryStoreDbContext dbContext, IRecipeMapper recipeMapper)
	{
		_dbContext = dbContext;
		_recipeMapper = recipeMapper;
	}

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
			.FirstOrDefaultAsync(item => item.Id == recipeId);

		return recipe is null ? null : _recipeMapper.ToRecipeDto(recipe);
	}
}