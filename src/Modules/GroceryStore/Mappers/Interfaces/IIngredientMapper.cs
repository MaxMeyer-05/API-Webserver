using GroceryStore.Models;
using GroceryStore.Database.Entities;

namespace GroceryStore.Mappers.Interfaces;
public interface IIngredientMapper
{
    /// <summary>
    /// Maps a <see cref="IngredientCreateDto"/> to a <see cref="Ingredient"/> entity.
    /// </summary>
    /// <param name="ingredientDto">The IngredientCreateDto to map.</param>
    /// <returns>The mapped Ingredient entity.</returns>
    Ingredient ToIngredientEntity(IngredientCreateDto ingredientDto);

    /// <summary>
    /// Maps a <see cref="Ingredient"/> entity to a <see cref="IngredientDto"/>.
    /// </summary>
    /// <param name="ingredient">The Ingredient entity to map.</param>
    /// <returns>The mapped IngredientDto.</returns>
    IngredientDto ToIngredientDto(Ingredient ingredient);

    /// <summary>
    /// Updates an existing <see cref="Ingredient"/> entity with values from a <see cref="IngredientUpdateDto"/>.
    /// </summary>
    /// <param name="ingredient">The <see cref="Ingredient"/> entity to update.</param>
    /// <param name="ingredientUpdateDto">The <see cref="IngredientUpdateDto"/> containing updated values.</param>
    void UpdateIngredientEntity(Ingredient ingredient, IngredientUpdateDto ingredientUpdateDto);
}