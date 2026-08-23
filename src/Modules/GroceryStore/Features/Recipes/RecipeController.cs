using Microsoft.AspNetCore.Mvc;

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

    public RecipeController(RecipeService recipeService)
    {
        _recipeService = recipeService;
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
        var recipe = await _recipeService.GetRecipeByIdAsync(recipeId);
        if (recipe is null)
        {
            return NotFound();
        }
        return Ok(recipe);
    }

    /// <summary>
    /// Creates a new recipe in the repository.
    /// </summary>
    /// <param name="recipe">The <see cref="RecipeCreateDto"/> containing the details of the recipe to be created.</param>
    /// <returns>Returns the created <see cref="RecipeDto"/> representing the newly created recipe.</returns>
    /// <response code="201">Returns the created <see cref="RecipeDto"/> representing the newly created recipe.</response>
    /// <response code="400">If the request body is invalid or if the recipe cannot be created due to business logic constraints.</response>
    /// <response code="403">If the supplier is not authorized to create the recipe.</response>
    [HttpPost]
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
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Adds a category to an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to which the category will be added.</param>
    /// <param name="categoryId">The ID of the category to add to the recipe.</param>
    /// <param name="supplierId">The ID of the supplier attempting to add the category.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <response code="204">If the category was successfully added to the recipe.</response>
    /// <response code="404">If the recipe or category does not exist.</response>
    /// <response code="403">If the supplier is not authorized to add the category to the recipe.</response>
    [HttpPost("{recipeId}/categories/{categoryId}/supplier/{supplierId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddCategoryToRecipe([FromRoute] int recipeId, [FromRoute] int categoryId, [FromRoute] Guid supplierId)
    {
        try
        {
            await _recipeService.AddCategoryToRecipeAsync(recipeId, categoryId, supplierId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Adds an ingredient to an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to which the ingredient will be added.</param>
    /// <param name="ingredientId">The ID of the ingredient to add to the recipe.</param>
    /// <param name="supplierId">The ID of the supplier attempting to add the ingredient.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <response code="204">If the ingredient was successfully added to the recipe.</response>
    /// <response code="404">If the recipe or ingredient does not exist.</response>
    /// <response code="403">If the supplier is not authorized to add the ingredient to the recipe.</response>
    [HttpPost("{recipeId}/ingredients/{ingredientId}/supplier/{supplierId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddIngredientToRecipe([FromRoute] int recipeId, [FromRoute] int ingredientId, [FromRoute] Guid supplierId)
    {
        try
        {
            await _recipeService.AddIngredientToRecipeAsync(recipeId, ingredientId, supplierId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Updates an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to update.</param>
    /// <param name="supplierId">The ID of the supplier attempting to update the recipe.</param>
    /// <param name="recipe">The <see cref="RecipeUpdateDto"/> containing the updated details of the recipe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <response code="204">If the recipe was successfully updated.</response>
    /// <response code="404">If the recipe does not exist.</response>
    /// <response code="403">If the supplier is not authorized to update the recipe.</response>
    [HttpPatch("{recipeId}/supplier/{supplierId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRecipe([FromRoute] int recipeId, [FromRoute] Guid supplierId, [FromBody] RecipeUpdateDto recipe)
    {
        try
        {
            await _recipeService.UpdateRecipeAsync(recipeId, supplierId, recipe);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Deletes an existing recipe from the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to delete.</param>
    /// <param name="supplierId">The ID of the supplier attempting to delete the recipe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <response code="204">If the recipe was successfully deleted.</response>
    /// <response code="404">If the recipe does not exist.</response>
    /// <response code="403">If the supplier is not authorized to delete the recipe.</response>
    [HttpDelete("{recipeId}/supplier/{supplierId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteRecipe([FromRoute] int recipeId, [FromRoute] Guid supplierId)
    {
        try
        {
            await _recipeService.DeleteRecipeAsync(recipeId, supplierId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Removes a category from an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe from which the category will be removed.</param>
    /// <param name="categoryId">The ID of the category to remove from the recipe.</param>
    /// <param name="supplierId">The ID of the supplier attempting to remove the category.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <response code="204">If the category was successfully removed from the recipe.</response>
    /// <response code="404">If the recipe or category does not exist.</response>
    /// <response code="403">If the supplier is not authorized to remove the category from the recipe.</response>
    [HttpDelete("{recipeId}/categories/{categoryId}/supplier/{supplierId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveCategoryFromRecipe([FromRoute] int recipeId, [FromRoute] int categoryId, [FromRoute] Guid supplierId)
    {
        try
        {
            await _recipeService.RemoveCategoryFromRecipeAsync(recipeId, categoryId, supplierId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Removes an ingredient from an existing recipe in the repository.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe from which the ingredient will be removed.</param>
    /// <param name="ingredientId">The ID of the ingredient to remove from the recipe.</param>
    /// <param name="supplierId">The ID of the supplier attempting to remove the ingredient.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <response code="204">If the ingredient was successfully removed from the recipe.</response>
    /// <response code="404">If the recipe or ingredient does not exist.</response>
    /// <response code="403">If the supplier is not authorized to remove the ingredient from the recipe.</response>
    [HttpDelete("{recipeId}/ingredients/{ingredientId}/supplier/{supplierId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveIngredientFromRecipe([FromRoute] int recipeId, [FromRoute] int ingredientId, [FromRoute] Guid supplierId)
    {
        try
        {
            await _recipeService.RemoveIngredientFromRecipeAsync(recipeId, ingredientId, supplierId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}