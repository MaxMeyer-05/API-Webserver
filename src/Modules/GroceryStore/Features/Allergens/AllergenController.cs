using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using SharedKernel.Security.Interfaces;
using GroceryStore.Features.Allergens.Interfaces;

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
    private readonly IAllergenService _allergenService;
    private readonly ICurrentUser _currentUser;

    public AllergenController(
        IAllergenService allergenService, 
        ICurrentUser currentUser)
    {
        _allergenService = allergenService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Retrieves all allergens from the repository.
    /// </summary>
    /// <returns>A list of all allergens.</returns>
    /// <response code="200">Returns a list of all allergens.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AllergenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AllergenDto>>> GetAllAllergens()
    {
        return Ok(await _allergenService.GetAllAllergensAsync());
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
    public async Task<ActionResult<AllergenDto>> GetAllergenById([FromRoute] int allergenId)
    {
        try
        {
            var allergen = await _allergenService.GetAllergenByIdAsync(allergenId);
            return Ok(allergen);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new allergen in the repository.
    /// </summary>
    /// <param name="allergen">The allergen to create.</param>
    /// <returns>The created allergen.</returns>
    /// <response code="201">Returns the created allergen.</response>
    /// <response code="400">If the allergen data is invalid.</response>
    [HttpPost("create")]
    [Authorize(Roles = Roles.Supplier)]
    [ProducesResponseType(typeof(AllergenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AllergenDto>> CreateAllergen([FromBody] AllergenCreateDto allergen)
    {
        try
        {
            var createdAllergen = await _allergenService.CreateAllergenAsync(allergen);
            return CreatedAtAction(nameof(GetAllergenById), new { allergenId = createdAllergen.AllergenId }, createdAllergen);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing allergen in the repository.
    /// </summary>
    /// <param name="allergenId">The ID of the allergen to update.</param>
    /// <param name="allergen">The updated allergen data.</param>
    /// <returns>No content if the update is successful.</returns>
    /// <response code="204">If the update is successful.</response>
    /// <response code="404">If the allergen is not found.</response>
    /// <response code="403">If the supplier is not authorized to update the allergen.</response>
    [HttpPatch("{allergenId}")]
    [Authorize(Roles = Roles.Supplier)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAllergen([FromRoute] int allergenId, [FromBody] AllergenUpdateDto allergen)
    {
        try
        {
            await _allergenService.UpdateAllergenAsync(allergenId, _currentUser.UserId, allergen);
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
    /// Deletes an allergen from the repository by its ID.
    /// </summary>
    /// <param name="allergenId">The ID of the allergen to delete.</param>
    /// <returns>No content if the deletion is successful.</returns>
    /// <response code="204">If the deletion is successful.</response>
    /// <response code="404">If the allergen is not found.</response>
    /// <response code="403">If the supplier is not authorized to delete the allergen.</response>
    [Authorize(Roles = Roles.Supplier)]
    [HttpDelete("{allergenId}/delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAllergen([FromRoute] int allergenId)
    {
        try
        {
            await _allergenService.DeleteAllergenAsync(allergenId, _currentUser.UserId);
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
}