using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;

using GroceryStore.Features.Recipes;
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
        var categoryEntity = await _dbContext.Categories.FindAsync(categoryId);
        if (categoryEntity == null)
        {
            _logger.LogDebug("Category with ID {CategoryId} not found", categoryId);
            throw new KeyNotFoundException($"Category with ID {categoryId} not found");
        }

        _dbContext.Categories.Remove(categoryEntity);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Deleted category with ID {CategoryId}", categoryId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _dbContext.Categories
            .Include(c => c.Recipes)
            .Select(c => new CategoryDto(
                c.Name,
                c.SupplierId,
                c.Recipes.Select(r => new RecipeRefDto(r.Name, r.SupplierId, r.Supplier.CompanyName)).ToList()
            ))
            .ToListAsync();

        _logger.LogDebug("Retrieved all categories from the database: {@Categories}", categories);
        return categories;
    }

    /// <inheritdoc />
    public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId)
    {
        var category = await _dbContext.Categories
            .Include(c => c.Recipes)
            .Where(c => c.Id == categoryId)
            .Select(c => new CategoryDto(
                c.Name,
                c.SupplierId,
                c.Recipes.Select(r => new RecipeRefDto(r.Name, r.SupplierId, r.Supplier.CompanyName)).ToList()
            ))
            .FirstOrDefaultAsync();

        _logger.LogDebug("Retrieved category with ID {CategoryId}: {@Category}", categoryId, category);
        return category;
    }

    /// <inheritdoc />
    public async Task UpdateCategoryAsync(int categoryId, CategoryUpdateDto category)
    {
        var categoryEntity = await _dbContext.Categories.FindAsync(categoryId);
        if (categoryEntity == null)
        {
            _logger.LogDebug("Category with ID {CategoryId} not found", categoryId);
            throw new KeyNotFoundException($"Category with ID {categoryId} not found");
        }

        _categoryMapper.UpdateCategoryEntity(categoryEntity, category);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated category with ID {CategoryId}", categoryId);
    }
}