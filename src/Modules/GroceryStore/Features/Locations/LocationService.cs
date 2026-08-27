using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;
using GroceryStore.Features.Locations.Interfaces;

namespace GroceryStore.Features.Locations;

/// <summary>
/// Represents the service responsible for managing locations in the grocery store application.
/// </summary>
public class LocationService : ILocationService
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly ILocationMapper _locationMapper;
    private readonly ILogger<LocationService> _logger;

    public LocationService(
        GroceryStoreDbContext dbContext,
        ILocationMapper locationMapper,
        ILogger<LocationService> logger)
    {
        _dbContext = dbContext;
        _locationMapper = locationMapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<LocationDto> CreateLocationAsync(LocationCreateDto location)
    {
        var locationEntity = _locationMapper.ToLocationEntity(location);

        _dbContext.Locations.Add(locationEntity);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new location with ZipCode {ZipCode}", locationEntity.ZipCode);

        return _locationMapper.ToLocationDto(locationEntity)
            ?? throw new InvalidOperationException("Failed to create location");
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task DeleteLocationAsync(string zipCode)
    {
        var location = await _dbContext.Locations
            .Where(l => l.ZipCode == zipCode)
            .ExecuteDeleteAsync();

        if (location == 0)
            throw new KeyNotFoundException($"Location with ZipCode {zipCode} not found");

        _logger.LogInformation("Deleted location with ZipCode {ZipCode}", zipCode);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LocationDto>> GetAllLocationsAsync()
    {
        var locations = await _dbContext.Locations.ToListAsync();
        return locations.Select(l => _locationMapper.ToLocationDto(l));
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<LocationDto?> GetLocationByZipCodeAsync(string zipCode)
    {
        var location = await _dbContext.Locations
            .Where(l => l.ZipCode == zipCode)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Location with ZipCode {zipCode} not found");

        return _locationMapper.ToLocationDto(location);
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task UpdateLocationAsync(string zipCode, LocationUpdateDto location)
    {
        var locationEntity = await _dbContext.Locations
            .Where(l => l.ZipCode == zipCode)
            .FirstOrDefaultAsync() 
            ?? throw new KeyNotFoundException($"Location with ZipCode {zipCode} not found");

        _locationMapper.UpdateLocationEntity(locationEntity, location);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated location with ZipCode {ZipCode}", zipCode);
    }
}