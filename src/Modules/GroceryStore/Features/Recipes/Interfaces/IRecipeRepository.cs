namespace GroceryStore.Features.Recipes.Interfaces;

public interface IRecipeRepository
{
	Task<RecipeDto?> GetRecipeByIdAsync(int recipeId);
}