using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;
using GroceryStore.Features.Categories.Interfaces;

namespace GroceryStore.Features.Categories;

/// <summary>
/// Represents a repository for managing categories in the database.
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly ICategoryMapper _categoryMapper;
    private readonly ILogger<CategoryRepository> _logger;

    public CategoryRepository(
        GroceryStoreDbContext dbContext,
        ICategoryMapper categoryMapper,
        ILogger<CategoryRepository> logger)
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

        var createdCategory = _categoryMapper.ToCategoryDto(categoryEntity);
        return createdCategory;
    }

    /// <inheritdoc />
    public async Task DeleteCategoryAsync(int categoryId)
    {
        var categoryEntity = await _dbContext.Categories.FindAsync(categoryId) 
            ?? throw new KeyNotFoundException($"Category with ID {categoryId} not found");
        
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

        _logger.LogDebug("Retrieved all categories from the database: {@Categories}", categories);
        return categories.Select(c => _categoryMapper.ToCategoryDto(c));
    }

    /// <inheritdoc />
    public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId)
    {
        var category = await _dbContext.Categories
            .Include(c => c.Recipes)
            .Where(c => c.Id == categoryId)
            .FirstOrDefaultAsync();

        _logger.LogDebug("Retrieved category with ID {CategoryId}: {@Category}", categoryId, category);
        return category == null ? null : _categoryMapper.ToCategoryDto(category);
    }

    /// <inheritdoc />
    public async Task UpdateCategoryAsync(int categoryId, CategoryUpdateDto category)
    {
        var categoryEntity = await _dbContext.Categories.FindAsync(categoryId) 
            ?? throw new KeyNotFoundException($"Category with ID {categoryId} not found");
        
        _categoryMapper.UpdateCategoryEntity(categoryEntity, category);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated category with ID {CategoryId}", categoryId);
    }
}