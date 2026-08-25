using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using SharedKernel.Security.Interfaces;
using GroceryStore.Features.Categories.Interfaces;

namespace GroceryStore.Features.Categories;

/// <summary>
/// Controller for managing categories in the grocery store module.
/// </summary>
[ApiController]
[Tags("Category")]
[Produces("application/json")]
[Route("api/module/grocery-store/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ICurrentUser _currentUser;

    public CategoryController(
        ICategoryService categoryService, 
        ICurrentUser currentUser)
    {
        _categoryService = categoryService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Retrieves all categories from the repository.
    /// </summary>
    /// <returns>A list of all categories.</returns>
    /// <response code="200">Returns a list of all categories.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
    {
        return Ok(await _categoryService.GetAllCategoriesAsync());
    }

    /// <summary>
    /// Retrieves a specific category by its ID.
    /// </summary>
    /// <param name="categoryId">The ID of the category to retrieve.</param>
    /// <returns>The category with the specified ID.</returns>
    /// <response code="200">Returns the category with the specified ID.</response>
    /// <response code="404">If the category is not found.</response>
    [HttpGet("{categoryId}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetCategoryById([FromRoute] int categoryId)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            return Ok(category);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new category in the repository.
    /// </summary>
    /// <param name="category">The category to create.</param>
    /// <returns>The created category.</returns>
    /// <response code="201">Returns the created category.</response>
    /// <response code="400">If the category data is invalid.</response>
    [HttpPost]
    [Authorize(Roles = "Supplier")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CategoryCreateDto category)
    {
        try
        {
            var createdCategory = await _categoryService.CreateCategoryAsync(category);
            return CreatedAtAction(nameof(GetCategoryById), createdCategory);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing category in the repository.
    /// </summary>
    /// <param name="categoryId">The ID of the category to update.</param>
    /// <param name="category">The updated category data.</param>
    /// <returns>No content if the update is successful.</returns>
    /// <response code="204">If the update is successful.</response>
    /// <response code="404">If the category is not found.</response>
    /// <response code="403">If the supplier is not authorized to update the category.</response>
    [HttpPatch("{categoryId}")]
    [Authorize(Roles = "Supplier")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateCategory([FromRoute] int categoryId, [FromBody] CategoryUpdateDto category)
    {
        try
        {
            await _categoryService.UpdateCategoryAsync(categoryId, _currentUser.UserId, category);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Deletes an existing category from the repository.
    /// </summary>
    /// <param name="categoryId">The ID of the category to delete.</param>
    /// <returns>No content if the deletion is successful.</returns>
    /// <response code="204">If the deletion is successful.</response>
    /// <response code="404">If the category is not found.</response>
    /// <response code="403">If the supplier is not authorized to delete the category.</response>
    [Authorize(Roles = "Supplier")]
    [HttpDelete("{categoryId}/delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteCategory([FromRoute] int categoryId)
    {
        try
        {
            await _categoryService.DeleteCategoryAsync(categoryId, _currentUser.UserId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}