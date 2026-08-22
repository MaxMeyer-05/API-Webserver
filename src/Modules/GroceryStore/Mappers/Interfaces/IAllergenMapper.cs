using GroceryStore.Models;
using GroceryStore.Database.Entities;

namespace GroceryStore.Mappers.Interfaces;
public interface IAllergenMapper
{
    /// <summary>
    /// Maps an <see cref="AllergenDto"/> to an <see cref="Allergen"/> entity.
    /// </summary>
    /// <param name="allergenDto">The AllergenDto to map.</param>
    /// <returns>The mapped Allergen entity.</returns>
    Allergen ToAllergenEntity(AllergenCreateDto allergenDto);

    /// <summary>
    /// Maps an <see cref="Allergen"/> entity to an <see cref="AllergenDto"/>.
    /// </summary>
    /// <param name="allergen">The Allergen entity to map.</param>
    /// <returns>The mapped AllergenDto.</returns>
    AllergenDto ToAllergenDto(Allergen allergen);


    /// <summary>
    /// Updates an existing <see cref="Allergen"/> entity with values from an <see cref="AllergenUpdateDto"/>.
    /// </summary>
    /// <param name="allergen">The <see cref="Allergen"/> entity to update.</param>
    /// <param name="allergenUpdateDto">The <see cref="AllergenUpdateDto"/> containing updated values.</param>
    void UpdateAllergenEntity(Allergen allergen, AllergenUpdateDto allergenUpdateDto);
}