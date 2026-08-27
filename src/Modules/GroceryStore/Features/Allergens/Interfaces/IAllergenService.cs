namespace GroceryStore.Features.Allergens.Interfaces;

/// <summary>
/// Defines the contract for a service that manages allergens in the database.
/// </summary>
public interface IAllergenService
{
    /// <summary>
    /// Retrieves all allergens from the database.
    /// </summary>
    /// <returns>Returns a collection of all allergens.</returns>
    Task<IEnumerable<AllergenDto>> GetAllAllergensAsync();

    /// <summary>
    /// Retrieves a specific allergen by its ID.
    /// </summary>
    /// <param name="allergenId">The ID of the allergen to retrieve.</param>
    /// <returns>Returns the allergen with the specified ID, or null if not found.</returns>
    Task<AllergenDto?> GetAllergenByIdAsync(int allergenId);

    /// <summary>
    /// Creates a new allergen in the database.
    /// </summary>
    /// <param name="allergen">The allergen to create.</param>
    /// <returns>Returns the created allergen.</returns>
    Task<AllergenDto> CreateAllergenAsync(AllergenCreateDto allergen);

    /// <summary>
    /// Updates an existing allergen in the database.
    /// </summary>
    /// <param name="allergenId">The ID of the allergen to update.</param>
    /// <param name="supplierId">The ID of the supplier requesting the update.</param>
    /// <param name="allergen">The updated allergen data.</param>
    Task UpdateAllergenAsync(int allergenId, Guid supplierId, AllergenUpdateDto allergen);

    /// <summary>
    /// Deletes an allergen from the database by its ID.
    /// </summary>
    /// <param name="allergenId">The ID of the allergen to delete.</param>
    /// <param name="supplierId">The ID of the supplier requesting the deletion.</param>
    Task DeleteAllergenAsync(int allergenId, Guid supplierId);
}