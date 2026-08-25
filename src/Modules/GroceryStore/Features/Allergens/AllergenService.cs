using GroceryStore.Features.Allergens.Interfaces;
using GroceryStore.Features.Suppliers.Interfaces;

namespace GroceryStore.Features.Allergens;

/// <summary>
/// Represents a service for managing allergens, 
/// providing business logic and validation for allergen-related operations.
/// </summary>
public class AllergenService
{
    private readonly IAllergenRepository _allergenRepository;
    private readonly ISupplierService _supplierService;
    private readonly ILogger<AllergenService> _logger;

    public AllergenService(
        IAllergenRepository allergenRepository, 
        ISupplierService supplierService, 
        ILogger<AllergenService> logger)
    {
        _allergenRepository = allergenRepository;
        _supplierService = supplierService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all allergens from the repository.
    /// </summary>
    /// <returns>Returns a list of all allergens.</returns>
    public async Task<IEnumerable<AllergenDto>> GetAllAllergensAsync()
    {
        _logger.LogDebug("Retrieving all allergens");
        return await _allergenRepository.GetAllAllergensAsync();
    }

    /// <summary>
    /// Retrieves a specific allergen by its ID.
    /// </summary>
    /// <param name="allergenId">The ID of the allergen to retrieve.</param>
    /// <returns>Returns the allergen with the specified ID, or null if not found.</returns>
    public async Task<AllergenDto?> GetAllergenByIdAsync(int allergenId)
    {
        _logger.LogDebug("Retrieving allergen with ID {AllergenId}", allergenId);
        return await _allergenRepository.GetAllergenByIdAsync(allergenId);
    }

    /// <summary>
    /// Creates a new allergen in the repository.
    /// </summary>
    /// <param name="allergen">The allergen to create.</param>
    /// <returns>Returns the created allergen.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to create the allergen.</exception>
    /// <remarks>Only suppliers are allowed to create allergens.</remarks>
    public async Task<AllergenDto> CreateAllergenAsync(AllergenCreateDto allergen)
    {
        _logger.LogDebug("Creating new allergen");
        _ = await _supplierService.GetSupplierByIdAsync(allergen.SupplierId) 
                ?? throw new UnauthorizedAccessException($"Only suppliers can create allergens. Supplier with ID {allergen.SupplierId} not found.");

        return await _allergenRepository.CreateAllergenAsync(allergen);
    }

    /// <summary>
    /// Updates an existing allergen in the repository.
    /// </summary>
    /// <param name="supplierId">The ID of the supplier attempting to update the allergen.</param>
    /// <param name="allergenId">The ID of the allergen to update.</param>
    /// <param name="allergen">The updated allergen data.</param>
    /// <returns>Returns a task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the allergen does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to update the allergen.</exception>
    /// <remarks>Only the supplier who created the allergen is allowed to update it.</remarks>
    public async Task UpdateAllergenAsync(Guid supplierId, int allergenId, AllergenUpdateDto allergen)
    {
        _logger.LogDebug("Updating allergen with ID {AllergenId}", allergenId);
        var existingAllergen = await _allergenRepository.GetAllergenByIdAsync(allergenId) 
            ?? throw new KeyNotFoundException($"Allergen with ID {allergenId} not found");

        if (existingAllergen.SupplierId != supplierId)
            throw new UnauthorizedAccessException($"Supplier with ID {supplierId} is not authorized to update allergen with ID {allergenId}");

        await _allergenRepository.UpdateAllergenAsync(allergenId, allergen);
    }

    /// <summary>
    /// Deletes an allergen from the repository by its ID.
    /// </summary>
    /// <param name="supplierId">The ID of the supplier attempting to delete the allergen.</param>
    /// <param name="allergenId">The ID of the allergen to delete.</param>
    /// <returns>Returns a task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the allergen does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the supplier is not authorized to delete the allergen.</exception>
    /// <remarks>Only the supplier who created the allergen is allowed to delete it.</remarks>
    public async Task DeleteAllergenAsync(Guid supplierId, int allergenId)
    {
        _logger.LogDebug("Deleting allergen with ID {AllergenId}", allergenId);
        var existingAllergen = await _allergenRepository.GetAllergenByIdAsync(allergenId) 
            ?? throw new KeyNotFoundException($"Allergen with ID {allergenId} not found");
            
        if (existingAllergen.SupplierId != supplierId)
            throw new UnauthorizedAccessException($"Supplier with ID {supplierId} is not authorized to delete allergen with ID {allergenId}");

        await _allergenRepository.DeleteAllergenAsync(allergenId);
    }
}