using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Features.Allergens;

/// <summary>
/// Controller for managing allergens in the grocery store module.
/// </summary>
[ApiController]
[Tags("Allergen")]
[Produces("application/json")]
[Route("api/module/grocery-store/allergens")]
public class AllergenController : ControllerBase
{
    private readonly AllergenService _allergenService;

    public AllergenController(AllergenService allergenService)
    {
        _allergenService = allergenService;
    }

    /// <summary>
    /// Retrieves all allergens from the repository.
    /// </summary>
    /// <returns>A list of all allergens.</returns>
    /// <response code="200">Returns a list of all allergens.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AllergenDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAllergens()
    {
        var allergens = await _allergenService.GetAllAllergensAsync();
        return Ok(allergens);
    }

    /// <summary>
    /// Retrieves a specific allergen by its ID.
    /// </summary>
    /// <param name="allergenId">The ID of the allergen to retrieve.</param>
    /// <returns>The allergen with the specified ID.</returns>
    /// <response code="200">Returns the allergen with the specified ID.</response>
    /// <response code="404">If the allergen is not found.</response>
    [HttpGet("{allergenId}")]
    [ProducesResponseType(typeof(AllergenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllergenById([FromRoute] int allergenId)
    {
        var allergen = await _allergenService.GetAllergenByIdAsync(allergenId);
        if (allergen == null)
        {
            return NotFound();
        }
        return Ok(allergen);
    }

    /// <summary>
    /// Creates a new allergen in the repository.
    /// </summary>
    /// <param name="allergen">The allergen to create.</param>
    /// <returns>The created allergen.</returns>
    /// <response code="201">Returns the created allergen.</response>
    /// <response code="400">If the allergen data is invalid.</response>
    /// <response code="403">If the supplier is not authorized to create the allergen.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AllergenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAllergen([FromBody] AllergenCreateDto allergen)
    {
        try
        {
            var createdAllergen = await _allergenService.CreateAllergenAsync(allergen);
            return CreatedAtAction(nameof(GetAllergenById), createdAllergen);
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
    /// Updates an existing allergen in the repository.
    /// </summary>
    /// <param name="supplierId">The ID of the supplier.</param>
    /// <param name="allergenId">The ID of the allergen to update.</param>
    /// <param name="allergen">The updated allergen data.</param>
    /// <returns>No content if the update is successful.</returns>
    /// <response code="204">If the update is successful.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the allergen is not found.</response>
    /// <response code="403">If the supplier is not authorized to update the allergen.</response>
    [HttpPatch("{allergenId}/suppliers/{supplierId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAllergen([FromRoute] Guid supplierId, [FromRoute] int allergenId, [FromBody] AllergenUpdateDto allergen)
    {
        try
        {
            await _allergenService.UpdateAllergenAsync(supplierId, allergenId, allergen);
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
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
    }

    /// <summary>
    /// Deletes an allergen from the repository by its ID.
    /// </summary>
    /// <param name="supplierId">The ID of the supplier.</param>
    /// <param name="allergenId">The ID of the allergen to delete.</param>
    /// <returns>No content if the deletion is successful.</returns>
    /// <response code="204">If the deletion is successful.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the allergen is not found.</response>
    /// <response code="403">If the supplier is not authorized to delete the allergen.</response>
    [HttpDelete("{allergenId}/suppliers/{supplierId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAllergen([FromRoute] Guid supplierId, [FromRoute] int allergenId)
    {
        try
        {
            await _allergenService.DeleteAllergenAsync(supplierId, allergenId);
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
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
    }
}