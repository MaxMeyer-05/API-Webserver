namespace GroceryStore.Features.Locations.Interfaces;

/// <summary>
/// Defines the contract for a service that manages locations in the grocery store application.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Retrieves all locations from the database.
    /// </summary>
    /// <returns>A collection of location DTOs.</returns>
    Task<IEnumerable<LocationDto>> GetAllLocationsAsync();

    /// <summary>
    /// Retrieves a specific location by its zip code.
    /// </summary>
    /// <param name="zipCode">The zip code of the location to retrieve.</param>
    /// <returns>The location DTO if found; otherwise, null.</returns>
    Task<LocationDto?> GetLocationByZipCodeAsync(string zipCode);

    /// <summary>
    /// Creates a new location in the database.
    /// </summary>
    /// <param name="location">The location DTO containing the data for the new location.</param>
    /// <returns>The created location DTO.</returns>
    Task<LocationDto> CreateLocationAsync(LocationCreateDto location);

    /// <summary>
    /// Updates an existing location in the database.
    /// </summary>
    /// <param name="zipCode">The zip code of the location to update.</param>
    /// <param name="location">The location DTO containing the updated data.</param>
    Task UpdateLocationAsync(string zipCode, LocationUpdateDto location);

    /// <summary>
    /// Deletes a location from the database by its zip code.
    /// </summary>
    /// <param name="zipCode">The zip code of the location to delete.</param>
    Task DeleteLocationAsync(string zipCode);
}