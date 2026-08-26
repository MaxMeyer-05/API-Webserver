using GroceryStore.Database.Entities;

using GroceryStore.Features.Recipes.Interfaces;
using GroceryStore.Features.Categories.Interfaces;
using GroceryStore.Features.Ingredients.Interfaces;

namespace GroceryStore.Features.Recipes;

/// <summary>
/// Mapper class for converting between Recipe entities and DTOs.
/// </summary>
public class RecipeMapper : IRecipeMapper
{
    private readonly ICategoryMapper _categoryMapper;
    private readonly IIngredientMapper _ingredientMapper;

    public RecipeMapper(
        ICategoryMapper categoryMapper, 
        IIngredientMapper ingredientMapper)
    {
        _categoryMapper = categoryMapper;
        _ingredientMapper = ingredientMapper;
    }
    /// <inheritdoc/>
    public RecipeDto ToRecipeDto(Recipe recipeEntity)
    {
        return new RecipeDto(
            RecipeId: recipeEntity.Id,
            SupplierRecipeCount: recipeEntity.Supplier.Recipes.Count,
            Name: recipeEntity.Name,
            Instructions: recipeEntity.Instructions,
            PreparationTime: recipeEntity.PreparationTime,
            SupplierId: recipeEntity.SupplierId,
            SupplierName: recipeEntity.Supplier.CompanyName,
            Categories: recipeEntity.Categories?.Select(_categoryMapper.ToCategoryDto).ToList() ?? [],
            Ingredients: recipeEntity.RecipeIngredients?.Select(ToRecipeIngredientDto).ToList() ?? []
        );
    }

    /// <inheritdoc/>
    public Recipe ToRecipeEntity(RecipeCreateDto recipeCreateDto)
    {
        return new Recipe
        {
            Name = recipeCreateDto.Name,
            Instructions = recipeCreateDto.Instructions,
            PreparationTime = recipeCreateDto.PreparationTime,
            SupplierId = recipeCreateDto.SupplierId,
            Categories = recipeCreateDto.CategoryIds?.Select(id => new Category { Id = id }).ToList() ?? [],
            RecipeIngredients = recipeCreateDto.Ingredients?.Select(ToRecipeIngredientEntity).ToList() ?? []
        };
    }

    /// <inheritdoc/>
    public RecipeIngredientDto ToRecipeIngredientDto(RecipeIngredient recipeIngredientEntity)
    {
        return new RecipeIngredientDto(
            Ingredient: _ingredientMapper.ToIngredientDto(recipeIngredientEntity.Ingredient),
            Amount: recipeIngredientEntity.Amount,
            SupplierId: recipeIngredientEntity.Ingredient.SupplierId,
            SupplierName: recipeIngredientEntity.Ingredient.Supplier.CompanyName
        );
    }

    /// <inheritdoc/>
    public RecipeIngredient ToRecipeIngredientEntity(RecipeIngredientItemCreateDto recipeIngredientItemCreateDto)
    {
        return new RecipeIngredient
        {
            RecipeId = recipeIngredientItemCreateDto.RecipeId,
            IngredientId = recipeIngredientItemCreateDto.IngredientId,
            Amount = recipeIngredientItemCreateDto.Amount
        };
    }

    /// <inheritdoc/>
    public void UpdateRecipeEntity(Recipe recipeEntity, RecipeUpdateDto recipeUpdateDto)
    {
        if (recipeUpdateDto.Name is not null)
        {
            recipeEntity.Name = recipeUpdateDto.Name;
        }

        if (recipeUpdateDto.Instructions is not null)
        {
            recipeEntity.Instructions = recipeUpdateDto.Instructions;
        }

        if (recipeUpdateDto.PreparationTime is not null)
        {
            recipeEntity.PreparationTime = recipeUpdateDto.PreparationTime.Value;
        }

        if (recipeUpdateDto.CategoryIds is not null)
        {
            recipeEntity.Categories = recipeUpdateDto.CategoryIds.Select(id => new Category { Id = id }).ToList();
        }

        if (recipeUpdateDto.Ingredients is not null)
        {
            recipeEntity.RecipeIngredients = recipeUpdateDto.Ingredients.Select(ToRecipeIngredientEntity).ToList();
        }
    }
}