using GroceryStore.Models;
using GroceryStore.Database.Entities;
using GroceryStore.Mappers.Interfaces;

namespace GroceryStore.Mappers;

/// <summary>
/// Mapper class for converting between Allergen entities and DTOs.
/// </summary>
public class AllergenMapper : IAllergenMapper
{
    /// <inheritdoc/>
    public AllergenDto ToAllergenDto(Allergen allergen)
    {
        return new AllergenDto(
            Name: allergen.Name,
            SupplierId: allergen.SupplierId,
            Ingredients: allergen.Ingredients?.Select(i => new IngredientRefDto(i.Name, i.SupplierId)).ToList()
        );
    }

    /// <inheritdoc/>
    public Allergen ToAllergenEntity(AllergenCreateDto allergenDto)
    {
        return new Allergen
        {
            Name = allergenDto.Name,
            SupplierId = allergenDto.SupplierId,
            Ingredients = allergenDto.IngredientIds?.Select(id => new Ingredient { Id = id }).ToList() ?? []
        };
    }

    /// <inheritdoc/>
    public void UpdateAllergenEntity(Allergen allergen, AllergenUpdateDto allergenUpdateDto)
    {
        if (allergenUpdateDto.Name is not null)
        {
            allergen.Name = allergenUpdateDto.Name;
        }

        if (allergenUpdateDto.IngredientIds is not null)
        {
            allergen.Ingredients = allergenUpdateDto.IngredientIds.Select(id => new Ingredient { Id = id }).ToList();
        }
    }
}