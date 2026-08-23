using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Features.Locations;

/// <summary>
/// Controller for managing locations in the grocery store module.
/// </summary>
[ApiController]
[Tags("Location")]
[Produces("application/json")]
[Route("api/module/grocery-store/locations")]
public class LocationController : ControllerBase
{
    private readonly LocationService _locationService;

    public LocationController(LocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// Retrieves all locations from the repository.
    /// </summary>
    /// <returns>A list of all locations.</returns>
    /// <response code="200">Returns a list of all locations.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LocationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LocationDto>>> GetAllLocations()
    {
        var locations = await _locationService.GetAllLocationsAsync();
        return Ok(locations);
    }

    /// <summary>
    /// Retrieves a specific location by its zip code.
    /// </summary>
    /// <param name="zipCode">The zip code of the location to retrieve.</param>
    /// <returns>The location with the specified zip code.</returns>
    /// <response code="200">Returns the location with the specified zip code.</response>
    /// <response code="404">If the location is not found.</response>
    [HttpGet("{zipCode}")]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocationDto>> GetLocationByZipCode([FromRoute] string zipCode)
    {
        var location = await _locationService.GetLocationByZipCodeAsync(zipCode);
        if (location == null)
        {
            return NotFound();
        }
        return Ok(location);
    }

    /// <summary>
    /// Creates a new location in the repository.
    /// </summary>
    /// <param name="location">The location to create.</param>
    /// <returns>The created location.</returns>
    /// <response code="201">Returns the created location.</response>
    /// <response code="400">If the location data is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LocationDto>> CreateLocation([FromBody] LocationCreateDto location)
    {
        try
        {
            var createdLocation = await _locationService.CreateLocationAsync(location);
            return CreatedAtAction(nameof(GetLocationByZipCode), createdLocation);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
    }

    /// <summary>
    /// Updates an existing location in the repository.
    /// </summary>
    /// <param name="zipCode">The zip code of the location to update.</param>
    /// <param name="location">The updated location data.</param>
    /// <returns>No content if the update is successful.</returns>
    /// <response code="204">If the update is successful.</response>
    /// <response code="404">If the location is not found.</response>
    [HttpPatch("{zipCode}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLocation([FromRoute] string zipCode, [FromBody] LocationUpdateDto location)
    {
        try
        {
            await _locationService.UpdateLocationAsync(zipCode, location);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Deletes a location from the repository.
    /// </summary>
    /// <param name="zipCode">The zip code of the location to delete.</param>
    /// <returns>No content if the deletion is successful.</returns>
    /// <response code="204">If the deletion is successful.</response>
    /// <response code="404">If the location is not found.</response>
    [HttpDelete("{zipCode}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLocation([FromRoute] string zipCode)
    {
        try
        {
            await _locationService.DeleteLocationAsync(zipCode);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}