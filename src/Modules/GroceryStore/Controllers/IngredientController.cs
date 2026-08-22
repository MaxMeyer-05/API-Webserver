using Microsoft.AspNetCore.Mvc;
using GroceryStore.DTOs;

namespace GroceryStore.Controllers;

[ApiController]
[Tags("GroceryStore Module")]
[Produces("application/json")]
[Route("api/module/grocery-store/ingredients")]
public class IngredientController : ControllerBase
{
    [HttpGet]
    public IActionResult GetIngredients()
    {
        return Ok();
    }

    [HttpGet("{ingredientId}")]
    public IActionResult GetIngredientById([FromRoute] int ingredientId)
    {
        return Ok();
    }

    [HttpPost]
    public IActionResult CreateIngredient([FromBody] IngredientCreateDto ingredient)
    {
        return Created();
    }

    [HttpPost("{ingredientId}/allergens/{allergenId}")]
    public IActionResult AddAllergenToIngredient([FromRoute] int ingredientId, [FromRoute] int allergenId)
    {
        return NoContent();
    }

    [HttpPatch("{ingredientId}")]
    public IActionResult UpdateIngredient([FromRoute] int ingredientId, [FromBody] IngredientUpdateDto ingredient)
    {
        return NoContent();
    }

    [HttpDelete("{ingredientId}")]
    public IActionResult DeleteIngredient([FromRoute] int ingredientId)
    {
        return NoContent();
    }

    [HttpDelete("{ingredientId}/allergens/{allergenId}")]
    public IActionResult RemoveAllergenFromIngredient([FromRoute] int ingredientId, [FromRoute] int allergenId)
    {
        return NoContent();
    }
}