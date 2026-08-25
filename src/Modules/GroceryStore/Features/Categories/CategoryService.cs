using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;
using GroceryStore.Features.Categories.Interfaces;

namespace GroceryStore.Features.Categories;

/// <summary>
/// Represents the service responsible for managing categories in the grocery store application.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly ICategoryMapper _categoryMapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        GroceryStoreDbContext dbContext,
        ICategoryMapper categoryMapper,
        ILogger<CategoryService> logger)
    {
        _dbContext = dbContext;
        _categoryMapper = categoryMapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto category)
    {
        var categoryEntity = _categoryMapper.ToCategoryEntity(category);

        _dbContext.Categories.Add(categoryEntity);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new category with ID {CategoryId}", categoryEntity.Id);

        return _categoryMapper.ToCategoryDto(categoryEntity)
            ?? throw new InvalidOperationException("Failed to map the created category entity to DTO.");
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task DeleteCategoryAsync(int categoryId, Guid supplierId)
    {
        var categoryEntity = await _dbContext.Categories
            .Where(c => c.Id == categoryId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Category with ID {categoryId} not found");

        if (categoryEntity.SupplierId != supplierId)
            throw new UnauthorizedAccessException($"Supplier with ID {supplierId} is not authorized to delete this category.");

        _dbContext.Categories.Remove(categoryEntity);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted category with ID {CategoryId}", categoryId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _dbContext.Categories
            .Include(c => c.Recipes)
            .ToListAsync();

        return categories.Select(c => _categoryMapper.ToCategoryDto(c));
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId)
    {
        var category = await _dbContext.Categories
            .Include(c => c.Recipes)
            .Where(c => c.Id == categoryId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Category with ID {categoryId} not found");

        return _categoryMapper.ToCategoryDto(category);
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task UpdateCategoryAsync(int categoryId, Guid supplierId, CategoryUpdateDto category)
    {
        var categoryEntity = await _dbContext.Categories.FindAsync(categoryId) 
            ?? throw new KeyNotFoundException($"Category with ID {categoryId} not found");

        if (categoryEntity.SupplierId != supplierId)
            throw new UnauthorizedAccessException($"Supplier with ID {supplierId} is not authorized to update this category.");
        
        _categoryMapper.UpdateCategoryEntity(categoryEntity, category);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated category with ID {CategoryId}", categoryId);
    }
}