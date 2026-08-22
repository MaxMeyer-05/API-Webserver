using Microsoft.AspNetCore.Mvc;
using GroceryStore.Models;

namespace GroceryStore.Controllers;

[ApiController]
[Tags("Categorie")]
[Produces("application/json")]
[Route("api/module/grocery-store/categories")]
public class CategorieController : ControllerBase
{
    [HttpGet]
    public IActionResult GetCategories()
    {
        return Ok();
    }

    [HttpGet("{categoryId}")]
    public IActionResult GetCategoryById([FromRoute] int categoryId)
    {
        return Ok();
    }

    [HttpPost]
    public IActionResult CreateCategory([FromBody] CategoryCreateDto category)
    {
        return Created();
    }

    [HttpPatch("{categoryId}")]
    public IActionResult UpdateCategory([FromRoute] int categoryId, [FromBody] CategoryUpdateDto category)
    {
        return NoContent();
    }

    [HttpDelete("{categoryId}")]
    public IActionResult DeleteCategory([FromRoute] int categoryId)
    {
        return NoContent();
    }
}