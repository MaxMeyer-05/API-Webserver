using GroceryStore.Models;
using GroceryStore.Database.Entities;
using GroceryStore.Mappers.Interfaces;

namespace GroceryStore.Mappers;

/// <summary>
/// Mapper class for converting between Category entities and DTOs.
/// </summary>
public class CategoryMapper : ICategoryMapper
{
    /// <inheritdoc/>
    public CategoryDto ToCategoryDto(Category category)
    {
        return new CategoryDto(
            Name: category.Name,
            SupplierId: category.SupplierId,
            Recipes: category.Recipes?.Select(r => new RecipeRefDto(r.Name, r.SupplierId, r.Supplier.CompanyName)).ToList()
        );
    }

    /// <inheritdoc/>
    public Category ToCategoryEntity(CategoryCreateDto categoryDto)
    {
        return new Category
        {
            Name = categoryDto.Name,
            SupplierId = categoryDto.SupplierId,
            Recipes = categoryDto.RecipeIds?.Select(id => new Recipe { Id = id }).ToList() ?? []
        };
    }

    /// <inheritdoc/>
    public void UpdateCategoryEntity(Category category, CategoryUpdateDto categoryUpdateDto)
    {
        if (categoryUpdateDto.Name is not null)
        {
            category.Name = categoryUpdateDto.Name;
        }

        if (categoryUpdateDto.RecipeIds is not null)
        {
            category.Recipes = categoryUpdateDto.RecipeIds.Select(id => new Recipe { Id = id }).ToList();
        }
    }
}