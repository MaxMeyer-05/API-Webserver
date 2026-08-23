using GroceryStore.Features.Locations.Interfaces;

namespace GroceryStore.Features.Locations;

/// <summary>
/// Represents a service for managing locations, 
/// providing business logic and validation for location-related operations.
/// </summary>
public class LocationService
{
    private readonly ILocationRepository _locationRepository;
    private readonly ILogger<LocationService> _logger;

    public LocationService(
        ILocationRepository locationRepository,
        ILogger<LocationService> logger)
    {
        _locationRepository = locationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all locations from the repository.
    /// </summary>
    /// <returns>Returns a collection of all locations.</returns>
    public async Task<IEnumerable<LocationDto>> GetAllLocationsAsync()
    {
        _logger.LogDebug("Retrieving all locations");
        return await _locationRepository.GetAllLocationsAsync();
    }

    /// <summary>
    /// Retrieves a specific location by its zip code.
    /// </summary>
    /// <param name="zipCode">The zip code of the location to retrieve.</param>
    /// <returns>Returns the location DTO if found; otherwise, null.</returns>
    public async Task<LocationDto?> GetLocationByZipCodeAsync(string zipCode)
    {
        _logger.LogDebug("Retrieving location with zip code {ZipCode}", zipCode);
        return await _locationRepository.GetLocationByZipCodeAsync(zipCode);
    }

    /// <summary>
    /// Creates a new location in the repository.
    /// </summary>
    /// <param name="location">The location create DTO containing the details of the location to create.</param>
    /// <returns>Returns the created location DTO.</returns>
    public async Task<LocationDto> CreateLocationAsync(LocationCreateDto location)
    {
        _logger.LogDebug("Creating new location");
        return await _locationRepository.CreateLocationAsync(location);
    }

    /// <summary>
    /// Updates an existing location in the repository.
    /// </summary>
    /// <param name="zipCode">The zip code of the location to update.</param>
    /// <param name="location">The location update DTO containing the updated details of the location.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the location with the specified zip code is not found.</exception>
    public async Task UpdateLocationAsync(string zipCode, LocationUpdateDto location)
    {
        _logger.LogDebug("Updating location with zip code {ZipCode}", zipCode);
        _ = await _locationRepository.GetLocationByZipCodeAsync(zipCode) 
            ?? throw new KeyNotFoundException($"Location with zip code {zipCode} not found.");

        await _locationRepository.UpdateLocationAsync(zipCode, location);
    }

    /// <summary>
    /// Deletes a location from the repository.
    /// </summary>
    /// <param name="zipCode">The zip code of the location to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the location with the specified zip code is not found.</exception>
    public async Task DeleteLocationAsync(string zipCode)
    {
        _logger.LogDebug("Deleting location with zip code {ZipCode}", zipCode);
        _ = await _locationRepository.GetLocationByZipCodeAsync(zipCode) 
            ?? throw new KeyNotFoundException($"Location with zip code {zipCode} not found.");

        await _locationRepository.DeleteLocationAsync(zipCode);
    }
}