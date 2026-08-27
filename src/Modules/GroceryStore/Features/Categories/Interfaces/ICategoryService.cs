namespace GroceryStore.Features.Categories.Interfaces;

/// <summary>
/// Defines the contract for a category service that provides operations for managing categories.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Retrieves all categories from the database.
    /// </summary>
    /// <returns>A collection of category DTOs.</returns>
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();

    /// <summary>
    /// Retrieves a specific category by its ID.
    /// </summary>
    /// <param name="categoryId">The ID of the category to retrieve.</param>
    /// <returns>The category DTO if found; otherwise, null.</returns>
    Task<CategoryDto?> GetCategoryByIdAsync(int categoryId);

    /// <summary>
    /// Creates a new category in the database.
    /// </summary>
    /// <param name="category">The category DTO containing the data for the new category.</param>
    /// <returns>The created category DTO.</returns>
    Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto category);

    /// <summary>
    /// Updates an existing category in the database.
    /// </summary>
    /// <param name="categoryId">The ID of the category to update.</param>
    /// <param name="supplierId">The ID of the supplier attempting to update the category.</param>
    /// <param name="category">The category DTO containing the updated data.</param>
    Task UpdateCategoryAsync(int categoryId, Guid supplierId, CategoryUpdateDto category);

    /// <summary>
    /// Deletes a category from the database by its ID.
    /// </summary>
    /// <param name="categoryId">The ID of the category to delete.</param>
    /// <param name="supplierId">The ID of the supplier attempting to delete the category.</param>
    Task DeleteCategoryAsync(int categoryId, Guid supplierId);
}