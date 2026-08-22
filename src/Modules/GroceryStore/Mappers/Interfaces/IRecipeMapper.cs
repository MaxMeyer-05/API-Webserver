using GroceryStore.Models;
using GroceryStore.Database.Entities;

namespace GroceryStore.Mappers.Interfaces;
public interface IRecipeMapper
{
    /// <summary>
    /// Maps a <see cref="RecipeCreateDto"/> to a <see cref="Recipe"/> entity.
    /// </summary>
    /// <param name="recipeCreateDto">The <see cref="RecipeCreateDto"/> to map.</param>
    /// <returns>The mapped <see cref="Recipe"/> entity.</returns>
    Recipe ToRecipeEntity(RecipeCreateDto recipeCreateDto);

    /// <summary>
    /// Maps a <see cref="Recipe"/> entity to a <see cref="RecipeDto"/>.
    /// </summary>
    /// <param name="recipeEntity">The <see cref="Recipe"/> entity to map.</param>
    /// <returns>The mapped <see cref="RecipeDto"/>.</returns>
    RecipeDto ToRecipeDto(Recipe recipeEntity);

    /// <summary>
    /// Updates an existing <see cref="Recipe"/> entity with values from a <see cref="RecipeUpdateDto"/>.
    /// </summary>
    /// <param name="recipeEntity">The <see cref="Recipe"/> entity to update.</param>
    /// <param name="recipeUpdateDto">The <see cref="RecipeUpdateDto"/> containing updated values.</param>
    void UpdateRecipeEntity(Recipe recipeEntity, RecipeUpdateDto recipeUpdateDto);


    /// <summary>
    /// Maps a <see cref="RecipeIngredientItemCreateDto"/> to a <see cref="RecipeIngredient"/> entity.
    /// </summary>
    /// <param name="recipeIngredientItemCreateDto">The <see cref="RecipeIngredientItemCreateDto"/> to map.</param>
    /// <returns>The mapped <see cref="RecipeIngredient"/> entity.</returns>
    RecipeIngredient ToRecipeIngredientEntity(RecipeIngredientItemCreateDto recipeIngredientItemCreateDto);

    /// <summary>
    /// Maps a <see cref="RecipeIngredient"/> entity to a <see cref="RecipeIngredientDto"/>.
    /// </summary>
    /// <param name="recipeIngredientEntity">The <see cref="RecipeIngredient"/> entity to map.</param>
    /// <returns>The mapped <see cref="RecipeIngredientDto"/>.</returns>
    RecipeIngredientDto ToRecipeIngredientDto(RecipeIngredient recipeIngredientEntity);
}