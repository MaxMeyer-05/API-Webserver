using GroceryStore.Models;
using GroceryStore.Database.Entities;

namespace GroceryStore.Mappers.Interfaces;
public interface ICategoryMapper
{
    /// <summary>
    /// Maps a <see cref="CategoryCreateDto"/> to a <see cref="Category"/> entity.
    /// </summary>
    /// <param name="categoryDto">The CategoryCreateDto to map.</param>
    /// <returns>The mapped Category entity.</returns>
    Category ToCategoryEntity(CategoryCreateDto categoryDto);

    /// <summary>
    /// Maps a <see cref="Category"/> entity to a <see cref="CategoryDto"/>.
    /// </summary>
    /// <param name="category">The Category entity to map.</param>
    /// <returns>The mapped CategoryDto.</returns>
    CategoryDto ToCategoryDto(Category category);

    /// <summary>
    /// Updates an existing <see cref="Category"/> entity with values from a <see cref="CategoryUpdateDto"/>.
    /// </summary>
    /// <param name="category">The <see cref="Category"/> entity to update.</param>
    /// <param name="categoryUpdateDto">The <see cref="CategoryUpdateDto"/> containing updated values.</param>
    void UpdateCategoryEntity(Category category, CategoryUpdateDto categoryUpdateDto);
}