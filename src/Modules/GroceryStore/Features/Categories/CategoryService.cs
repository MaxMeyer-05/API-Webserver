using GroceryStore.Features.Categories.Interfaces;
using GroceryStore.Features.Suppliers.Interfaces;

namespace GroceryStore.Features.Categories;

/// <summary>
/// Represents a service for managing categories, 
/// providing business logic and validation for category-related operations.
/// </summary>
public class CategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISupplierService _supplierService;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository, 
        ISupplierService supplierService, 
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _supplierService = supplierService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all categories from the repository.
    /// </summary>
    /// <returns>Returns a collection of category DTOs.</returns>
    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        _logger.LogDebug("Retrieving all categories");
        return await _categoryRepository.GetAllCategoriesAsync();
    }

    /// <summary>
    /// Retrieves a specific category by its ID.
    /// </summary>
    /// <param name="categoryId">The ID of the category to retrieve.</param>
    /// <returns>Returns the category DTO if found; otherwise, null.</returns>
    public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId)
    {
        _logger.LogDebug("Retrieving category with ID {CategoryId}", categoryId);
        return await _categoryRepository.GetCategoryByIdAsync(categoryId);
    }

    /// <summary>
    /// Creates a new category in the repository.
    /// </summary>
    /// <param name="category">The category create DTO containing the details of the category to create.</param>
    /// <returns>Returns the created category DTO.</returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto category)
    {
        _logger.LogDebug("Creating new category");
        _ = await _supplierService.GetSupplierByIdAsync(category.SupplierId) 
            ?? throw new UnauthorizedAccessException("Only suppliers are allowed to create categories.");
        return await _categoryRepository.CreateCategoryAsync(category);
    }

    /// <summary>
    /// Updates an existing category in the repository.
    /// </summary>
    /// <param name="supplierId">The ID of the supplier attempting to update the category.</param>
    /// <param name="categoryId">The ID of the category to update.</param>
    /// <param name="category">The category update DTO containing the new details of the category.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the category with the specified ID does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the supplier is not authorized to update the category.</exception>
    /// <remarks>Only the supplier who owns the category can update it.</remarks>
    public async Task UpdateCategoryAsync(Guid supplierId, int categoryId, CategoryUpdateDto category)
    {
        _logger.LogDebug("Updating category with ID {CategoryId}", categoryId);
        var existingCategory = await _categoryRepository.GetCategoryByIdAsync(categoryId) 
            ?? throw new KeyNotFoundException($"Category with ID {categoryId} not found.");

        if (existingCategory.SupplierId != supplierId)
            throw new UnauthorizedAccessException($"Supplier with ID {supplierId} is not authorized to update this category.");

        await _categoryRepository.UpdateCategoryAsync(categoryId, category);
    }

    /// <summary>
    /// Deletes an existing category from the repository.
    /// </summary>
    /// <param name="supplierId">The ID of the supplier attempting to delete the category.</param>
    /// <param name="categoryId">The ID of the category to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the category with the specified ID does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the supplier is not authorized to delete the category.</exception>
    /// <remarks>Only the supplier who owns the category can delete it.</remarks>
    public async Task DeleteCategoryAsync(Guid supplierId, int categoryId)
    {
        _logger.LogDebug("Deleting category with ID {CategoryId}", categoryId);
        var existingCategory = await _categoryRepository.GetCategoryByIdAsync(categoryId) 
            ?? throw new KeyNotFoundException($"Category with ID {categoryId} not found.");

        if (existingCategory.SupplierId != supplierId)
            throw new UnauthorizedAccessException($"Supplier with ID {supplierId} is not authorized to delete this category.");

        await _categoryRepository.DeleteCategoryAsync(categoryId);
    }
}