using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Features.Recipes;

[ApiController]
[Tags("Recipe")]
[Produces("application/json")]
[Route("api/module/grocery-store/recipes")]
public class RecipeController : ControllerBase
{
    [HttpGet]
    public IActionResult GetRecipes()
    {
        return Ok();
    }

    [HttpGet("{recipeId}")]
    public IActionResult GetRecipeById([FromRoute] int recipeId)
    {
        return Ok();
    }

    [HttpPost]
    public IActionResult CreateRecipe([FromBody] RecipeCreateDto recipe)
    {
        return Created();
    }

    [HttpPost("{recipeId}/categories/{categoryId}")]
    public IActionResult AddCategoryToRecipe([FromRoute] int recipeId, [FromRoute] int categoryId)
    {
        return Ok();
    }

    [HttpPost("{recipeId}/ingredients/{ingredientId}")]
    public IActionResult AddIngredientToRecipe([FromRoute] int recipeId, [FromRoute] int ingredientId)
    {
        return Ok();
    }

    [HttpPatch("{recipeId}")]
    public IActionResult UpdateRecipe([FromRoute] int recipeId, [FromBody] RecipeUpdateDto recipe)
    {
        return NoContent();
    }

    [HttpDelete("{recipeId}")]
    public IActionResult DeleteRecipe([FromRoute] int recipeId)
    {
        return NoContent();
    }

    [HttpDelete("{recipeId}/categories/{categoryId}")]
    public IActionResult RemoveCategoryFromRecipe([FromRoute] int recipeId, [FromRoute] int categoryId)
    {
        return NoContent();
    }

    [HttpDelete("{recipeId}/ingredients/{ingredientId}")]
    public IActionResult RemoveIngredientFromRecipe([FromRoute] int recipeId, [FromRoute] int ingredientId)
    {
        return NoContent();
    }
}