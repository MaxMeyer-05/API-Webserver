using Microsoft.AspNetCore.Mvc;
using GroceryStore.DTOs;

namespace GroceryStore.Controllers;

[ApiController]
[Tags("Location")]
[Produces("application/json")]
[Route("api/module/grocery-store/locations")]
public class LocationController : ControllerBase
{
    [HttpGet]
    public IActionResult GetLocations()
    {
        return Ok();
    }

    [HttpGet("{zipCode}")]
    public IActionResult GetLocationById([FromRoute] int zipCode)
    {
        return Ok();
    }

    [HttpPost]
    public IActionResult CreateLocation([FromBody] LocationCreateDto location)
    {
        return Created();
    }

    [HttpPatch("{zipCode}")]
    public IActionResult UpdateLocation([FromRoute] int zipCode, [FromBody] LocationUpdateDto location)
    {
        return NoContent();
    }

    [HttpDelete("{zipCode}")]
    public IActionResult DeleteLocation([FromRoute] int zipCode)
    {
        return NoContent();
    }
}