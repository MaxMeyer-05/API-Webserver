namespace GroceryStore.Features.Categories.Interfaces;

/// <summary>
/// Defines the contract for a repository that manages categories in the database.
/// </summary>
public interface ICategoryRepository
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
    /// <param name="category">The category DTO containing the updated data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateCategoryAsync(int categoryId, CategoryUpdateDto category);

    /// <summary>
    /// Deletes a category from the database by its ID.
    /// </summary>
    /// <param name="categoryId">The ID of the category to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteCategoryAsync(int categoryId);
}