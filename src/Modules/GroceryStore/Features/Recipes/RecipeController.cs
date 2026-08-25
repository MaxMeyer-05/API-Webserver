using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using SharedKernel.Security.Interfaces;

namespace GroceryStore.Features.Recipes;

/// <summary>
/// Represents the controller responsible for handling recipe-related API endpoints.
/// </summary>
[ApiController]
[Tags("Recipe")]
[Produces("application/json")]
[Route("api/module/grocery-store/recipes")]
public class RecipeController : ControllerBase
{
    private readonly RecipeService _recipeService;
    private readonly ICurrentUser _currentUser;

    public RecipeController(
        RecipeService recipeService, 
        ICurrentUser currentUser)
    {
        _recipeService = recipeService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Retrieves all recipes from the repository.
    /// </summary>
    /// <returns>Returns a collection of <see cref="RecipeDto"/> representing all recipes.</returns>
    /// <response code="200">Returns a collection of <see cref="RecipeDto"/> representing all recipes.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RecipeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> GetAllRecipes()
    {
        return Ok(await _recipeService.GetAllRecipesAsync());
    }

    /// <summary>
    /// Retrieves a specific recipe by its ID.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to retrieve.</param>
    /// <returns>Returns the <see cref="RecipeDto"/> representing the requested recipe.</returns>
    /// <response code="200">Returns the <see cref="RecipeDto"/> representing the requested recipe.</response>
    /// <response code="404">If the recipe with the specified ID does not exist.</response>
    [HttpGet("{recipeId}")]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeDto>> GetRecipeById([FromRoute] int recipeId)
    {
        try
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(recipeId);
            return Ok(recipe);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new recipe in the repository.
    /// </summary>
    /// <param name="recipe">The <see cref="RecipeCreateDto"/> containing the details of the recipe to be created.</param>
    /// <returns>Returns the created <see cref="RecipeDto"/> representing the newly created recipe.</returns>
    /// <response code="201">Returns the created <see cref="RecipeDto"/> representing the newly created recipe.</response>
    /// <response code="400">If the request body is invalid or if the recipe cannot be created due to business logic constraints.</response>
    /// <response code="403">If the supplier is not authorized to create the recipe.</response>
    [HttpPost("create")]
    [Authorize(Roles = "Supplier")]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RecipeDto>> CreateRecipe([FromBody] RecipeCreateDto recipe)
    {
        try
        {
            var createdRecipe = await _recipeService.CreateRecipeAsync(recipe);
            return CreatedAtAction(nameof(GetRecipeById), createdRecipe);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Adds a category to an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to which the category will be added.</param>
    /// <param name="categoryId">The ID of the category to add to the recipe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <response code="204">If the category was successfully added to the recipe.</response>
    /// <response code="400">If the category cannot be added due to business logic constraints.</response>
    /// <response code="404">If the recipe or category does not exist.</response>
    /// <response code="403">If the supplier is not authorized to add the category to the recipe.</response>
    [Authorize(Roles = "Supplier")]
    [HttpPost("{recipeId}/categories/{categoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddCategoryToRecipe([FromRoute] int recipeId, [FromRoute] int categoryId)
    {
        try
        {
            await _recipeService.AddCategoryToRecipeAsync(recipeId, categoryId, _currentUser.UserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Adds an ingredient to an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to which the ingredient will be added.</param>
    /// <param name="ingredientId">The ID of the ingredient to add to the recipe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <response code="204">If the ingredient was successfully added to the recipe.</response>
    /// <response code="400">If the ingredient cannot be added due to business logic constraints.</response>
    /// <response code="404">If the recipe or ingredient does not exist.</response>
    /// <response code="403">If the supplier is not authorized to add the ingredient to the recipe.</response>
    [Authorize(Roles = "Supplier")]
    [HttpPost("{recipeId}/ingredients/{ingredientId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddIngredientToRecipe([FromRoute] int recipeId, [FromRoute] int ingredientId)
    {
        try
        {
            await _recipeService.AddIngredientToRecipeAsync(recipeId, ingredientId, _currentUser.UserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to update.</param>
    /// <param name="recipe">The <see cref="RecipeUpdateDto"/> containing the updated details of the recipe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <response code="204">If the recipe was successfully updated.</response>
    /// <response code="404">If the recipe does not exist.</response>
    /// <response code="403">If the supplier is not authorized to update the recipe.</response>
    [HttpPatch("{recipeId}")]
    [Authorize(Roles = "Supplier")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRecipe([FromRoute] int recipeId, [FromBody] RecipeUpdateDto recipe)
    {
        try
        {
            await _recipeService.UpdateRecipeAsync(recipeId, _currentUser.UserId, recipe);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Deletes an existing recipe from the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <response code="204">If the recipe was successfully deleted.</response>
    /// <response code="404">If the recipe does not exist.</response>
    /// <response code="403">If the supplier is not authorized to delete the recipe.</response>
    [HttpDelete("{recipeId}/delete")]
    [Authorize(Roles = "Supplier")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteRecipe([FromRoute] int recipeId)
    {
        try
        {
            await _recipeService.DeleteRecipeAsync(recipeId, _currentUser.UserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Removes a category from an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe from which the category will be removed.</param>
    /// <param name="categoryId">The ID of the category to remove from the recipe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <response code="204">If the category was successfully removed from the recipe.</response>
    /// <response code="400">If the category cannot be removed due to business logic constraints.</response>
    /// <response code="404">If the recipe or category does not exist.</response>
    /// <response code="403">If the supplier is not authorized to remove the category from the recipe.</response>
    [Authorize(Roles = "Supplier")]
    [HttpDelete("{recipeId}/categories/{categoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveCategoryFromRecipe([FromRoute] int recipeId, [FromRoute] int categoryId)
    {
        try
        {
            await _recipeService.RemoveCategoryFromRecipeAsync(recipeId, categoryId, _currentUser.UserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Removes an ingredient from an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe from which the ingredient will be removed.</param>
    /// <param name="ingredientId">The ID of the ingredient to remove from the recipe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <response code="204">If the ingredient was successfully removed from the recipe.</response>
    /// <response code="400">If the ingredient cannot be removed due to business logic constraints.</response>
    /// <response code="404">If the recipe or ingredient does not exist.</response>
    /// <response code="403">If the supplier is not authorized to remove the ingredient from the recipe.</response>
    [Authorize(Roles = "Supplier")]
    [HttpDelete("{recipeId}/ingredients/{ingredientId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveIngredientFromRecipe([FromRoute] int recipeId, [FromRoute] int ingredientId)
    {
        try
        {
            await _recipeService.RemoveIngredientFromRecipeAsync(recipeId, ingredientId, _currentUser.UserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}