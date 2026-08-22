using GroceryStore.Models;
using GroceryStore.Database.Entities;
using GroceryStore.Mappers.Interfaces;

namespace GroceryStore.Mappers;

/// <summary>
/// Mapper class for converting between Recipe entities and DTOs.
/// </summary>
public class RecipeMapper : IRecipeMapper
{
    /// <inheritdoc/>
    public RecipeDto ToRecipeDto(Recipe recipeEntity)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Recipe ToRecipeEntity(RecipeCreateDto recipeCreateDto)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public RecipeIngredientDto ToRecipeIngredientDto(RecipeIngredient recipeIngredientEntity)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public RecipeIngredient ToRecipeIngredientEntity(RecipeIngredientItemCreateDto recipeIngredientItemCreateDto)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void UpdateRecipeEntity(Recipe recipeEntity, RecipeUpdateDto recipeUpdateDto)
    {
        throw new NotImplementedException();
    }
}