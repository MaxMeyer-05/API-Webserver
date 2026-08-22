using Microsoft.AspNetCore.Mvc;
using GroceryStore.DTOs;

namespace GroceryStore.Controllers;

[ApiController]
[Tags("Allergen")]
[Produces("application/json")]
[Route("api/module/grocery-store/allergens")]
public class AllergenController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllergens()
    {
        return Ok();
    }

    [HttpGet("{allergenId}")]
    public IActionResult GetAllergenById([FromRoute] int allergenId)
    {
        return Ok();
    }

    [HttpPost]
    public IActionResult CreateAllergen([FromBody] AllergenCreateDto allergen)
    {
        return Created();
    }

    [HttpPatch("{allergenId}")]
    public IActionResult UpdateAllergen([FromRoute] int allergenId, [FromBody] AllergenUpdateDto allergen)
    {
        return NoContent();
    }

    [HttpDelete("{allergenId}")]
    public IActionResult DeleteAllergen([FromRoute] int allergenId)
    {
        return NoContent();
    }
}