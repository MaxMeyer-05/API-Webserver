using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;
using GroceryStore.Features.Locations.Interfaces;

namespace GroceryStore.Features.Locations;

/// <summary>
/// Represents a repository for managing locations in the database.
/// </summary>
public class LocationRepository : ILocationRepository
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly ILocationMapper _locationMapper;
    private readonly ILogger<LocationRepository> _logger;

    public LocationRepository(
        GroceryStoreDbContext dbContext,
        ILocationMapper locationMapper,
        ILogger<LocationRepository> logger)
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

        var createdLocation = _locationMapper.ToLocationDto(locationEntity);
        return createdLocation;
    }

    /// <inheritdoc />
    public async Task DeleteLocationAsync(string zipCode)
    {
        var location = await _dbContext.Locations
            .Where(l => l.ZipCode == zipCode)
            .FirstOrDefaultAsync() 
            ?? throw new KeyNotFoundException($"Location with ZipCode {zipCode} not found");
        
        _dbContext.Locations.Remove(location);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Deleted location with ZipCode {ZipCode}", zipCode);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LocationDto>> GetAllLocationsAsync()
    {
        var locations = await _dbContext.Locations.ToListAsync();
        return locations.Select(l => _locationMapper.ToLocationDto(l));
    }

    /// <inheritdoc />
    public async Task<LocationDto?> GetLocationByZipCodeAsync(string zipCode)
    {
        var location = await _dbContext.Locations
            .Where(l => l.ZipCode == zipCode)
            .FirstOrDefaultAsync();

        return location != null ? _locationMapper.ToLocationDto(location) : null;
    }

    /// <inheritdoc />
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