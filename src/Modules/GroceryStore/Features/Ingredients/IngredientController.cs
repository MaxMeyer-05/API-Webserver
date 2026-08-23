using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Features.Ingredients;

/// <summary>
/// Controller for managing ingredients in the grocery store module.
/// </summary>
[ApiController]
[Tags("Ingredient")]
[Produces("application/json")]
[Route("api/module/grocery-store/ingredients")]
public class IngredientController : ControllerBase
{
    private readonly IngredientService _ingredientService;

    public IngredientController(IngredientService ingredientService)
    {
        _ingredientService = ingredientService;
    }

    /// <summary>
    /// Retrieves all ingredients from the repository.
    /// </summary>
    /// <returns>A list of all ingredients.</returns>
    /// <response code="200">Returns a list of all ingredients.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<IngredientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllIngredients()
    {
        var ingredients = await _ingredientService.GetAllIngredientsAsync();
        return Ok(ingredients);
    }

    /// <summary>
    /// Retrieves a specific ingredient by its ID.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient to retrieve.</param>
    /// <returns>The ingredient with the specified ID.</returns>
    /// <response code="200">Returns the ingredient with the specified ID.</response>
    /// <response code="404">If the ingredient is not found.</response>
    [HttpGet("{ingredientId}")]
    [ProducesResponseType(typeof(IngredientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIngredientById([FromRoute] int ingredientId)
    {
        var ingredient = await _ingredientService.GetIngredientByIdAsync(ingredientId);
        if (ingredient == null)
        {
            return NotFound();
        }
        return Ok(ingredient);
    }

    /// <summary>
    /// Creates a new ingredient in the repository.
    /// </summary>
    /// <param name="ingredient">The ingredient to create.</param>
    /// <returns>The created ingredient.</returns>
    /// <response code="201">Returns the created ingredient.</response>
    /// <response code="400">If the ingredient data is invalid.</response>
    /// <response code="403">If the supplier is not authorized to create the ingredient.</response>
    [HttpPost]
    [ProducesResponseType(typeof(IngredientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateIngredient([FromBody] IngredientCreateDto ingredient)
    {
        try
        {
            var createdIngredient = await _ingredientService.CreateIngredientAsync(ingredient);
            return CreatedAtAction(nameof(GetIngredientById), createdIngredient);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
    }

    /// <summary>
    /// Adds an allergen to a specific ingredient.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient.</param>
    /// <param name="allergenId">The ID of the allergen to add.</param>
    /// <param name="supplierId">The ID of the supplier making the request.</param>
    /// <returns>No content if the operation is successful.</returns>
    /// <response code="204">If the allergen is successfully added to the ingredient.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the ingredient or allergen is not found.</response>
    /// <response code="403">If the supplier is not authorized to add the allergen to the ingredient.</response>
    [HttpPost("{ingredientId}/allergens/{allergenId}/suppliers/{supplierId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddAllergenToIngredient([FromRoute] int ingredientId, [FromRoute] int allergenId, [FromRoute] Guid supplierId)
    {
        try
        {
            await _ingredientService.AddAllergenToIngredientAsync(ingredientId, allergenId, supplierId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
    }

    /// <summary>
    /// Updates an existing ingredient in the repository.
    /// </summary>
    /// <param name="supplierId">The ID of the supplier.</param>
    /// <param name="ingredientId">The ID of the ingredient to update.</param>
    /// <param name="ingredient">The updated ingredient data.</param>
    /// <returns>No content if the update is successful.</returns>
    /// <response code="204">If the update is successful.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the ingredient is not found.</response>
    /// <response code="403">If the supplier is not authorized to update the ingredient.</response>
    [HttpPatch("{ingredientId}/suppliers/{supplierId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateIngredient([FromRoute] Guid supplierId, [FromRoute] int ingredientId, [FromBody] IngredientUpdateDto ingredient)
    {
        try
        {
            await _ingredientService.UpdateIngredientAsync(supplierId, ingredientId, ingredient);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
    }

    /// <summary>
    /// Deletes an existing ingredient from the repository.
    /// </summary>
    /// <param name="supplierId">The ID of the supplier.</param>
    /// <param name="ingredientId">The ID of the ingredient to delete.</param>
    /// <returns>No content if the deletion is successful.</returns>
    /// <response code="204">If the deletion is successful.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the ingredient is not found.</response>
    /// <response code="403">If the supplier is not authorized to delete the ingredient.</response>
    [HttpDelete("{ingredientId}/suppliers/{supplierId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteIngredient([FromRoute] Guid supplierId, [FromRoute] int ingredientId)
    {
        try
        {
            await _ingredientService.DeleteIngredientAsync(supplierId, ingredientId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
    }

    /// <summary>
    /// Removes an allergen from a specific ingredient.
    /// </summary>
    /// <param name="ingredientId">The ID of the ingredient.</param>
    /// <param name="allergenId">The ID of the allergen to remove.</param>
    /// <param name="supplierId">The ID of the supplier making the request.</param>
    /// <returns>No content if the operation is successful.</returns>
    /// <response code="204">If the allergen is successfully removed from the ingredient.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the ingredient or allergen is not found.</response>
    /// <response code="403">If the supplier is not authorized to remove the allergen from the ingredient.</response>
    [HttpDelete("{ingredientId}/allergens/{allergenId}/suppliers/{supplierId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveAllergenFromIngredient([FromRoute] int ingredientId, [FromRoute] int allergenId, [FromRoute] Guid supplierId)
    {
        try
        {
            await _ingredientService.RemoveAllergenFromIngredientAsync(ingredientId, allergenId, supplierId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
    }
}