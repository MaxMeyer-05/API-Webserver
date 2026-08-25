using GroceryStore.Database.Entities;

using GroceryStore.Features.Allergens;
using GroceryStore.Features.Ingredients.Interfaces;

namespace GroceryStore.Features.Ingredients;

/// <summary>
/// Mapper class for converting between Ingredient entities and DTOs.
/// </summary>
public class IngredientMapper : IIngredientMapper
{
    /// <inheritdoc/>
    public IngredientDto ToIngredientDto(Ingredient ingredient)
    {
        return new IngredientDto(
            IngredientId: ingredient.Id,
            SupplierIngredientCount: ingredient.Supplier?.Ingredients.Count ?? 0,
            Name: ingredient.Name,
            Unit: ingredient.Unit,
            NetPrice: ingredient.NetPrice,
            Stock: ingredient.Stock,
            SupplierId: ingredient.SupplierId,
            SupplierName: ingredient.Supplier?.CompanyName ?? string.Empty,
            Calories: ingredient.Calories,
            Carbohydrates: ingredient.Carbohydrates,
            Protein: ingredient.Protein,
            Allergens: ingredient.Allergens?.Select(a => new AllergenDto(a.Name, a.SupplierId)).ToList()
        );
    }

    /// <inheritdoc/>
    public Ingredient ToIngredientEntity(IngredientCreateDto ingredientDto)
    {
        return new Ingredient
        {
            Name = ingredientDto.Name,
            Unit = ingredientDto.Unit,
            NetPrice = ingredientDto.NetPrice,
            Stock = ingredientDto.Stock,
            SupplierId = ingredientDto.SupplierId,
            Calories = ingredientDto.Calories,
            Carbohydrates = ingredientDto.Carbohydrates,
            Protein = ingredientDto.Protein,
            Allergens = ingredientDto.AllergenIds?.Select(id => new Allergen { Id = id }).ToList() ?? []
        };
    }

    /// <inheritdoc/>
    public void UpdateIngredientEntity(Ingredient ingredient, IngredientUpdateDto ingredientUpdateDto)
    {
        if (ingredientUpdateDto.Name is not null)
        {
            ingredient.Name = ingredientUpdateDto.Name;
        }

        if (ingredientUpdateDto.Unit is not null)
        {
            ingredient.Unit = ingredientUpdateDto.Unit;
        }

        if (ingredientUpdateDto.NetPrice is not null)
        {
            ingredient.NetPrice = ingredientUpdateDto.NetPrice.Value;
        }

        if (ingredientUpdateDto.Stock is not null)
        {
            ingredient.Stock = ingredientUpdateDto.Stock.Value;
        }

        if (ingredientUpdateDto.Calories is not null)
        {
            ingredient.Calories = ingredientUpdateDto.Calories;
        }

        if (ingredientUpdateDto.Carbohydrates is not null)
        {
            ingredient.Carbohydrates = ingredientUpdateDto.Carbohydrates;
        }

        if (ingredientUpdateDto.Protein is not null)
        {
            ingredient.Protein = ingredientUpdateDto.Protein;
        }

        if (ingredientUpdateDto.AllergenIds is not null)
        {
            ingredient.Allergens = ingredientUpdateDto.AllergenIds.Select(id => new Allergen { Id = id }).ToList();
        }
    }
}