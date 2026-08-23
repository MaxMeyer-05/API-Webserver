using GroceryStore.Database.Entities;
using GroceryStore.Features.Locations.Interfaces;

namespace GroceryStore.Features.Locations;

/// <summary>
/// Mapper class for converting between Location entities and DTOs.
/// </summary>
public class LocationMapper : ILocationMapper
{
    /// <inheritdoc/>
    public LocationDto ToLocationDto(Location location)
    {
        return new LocationDto(            
            location.ZipCode,
            location.City
        );
    }

    /// <inheritdoc/>
    public Location ToLocationEntity(LocationCreateDto locationDto)
    {
        return new Location
        {
            ZipCode = locationDto.ZipCode,
            City = locationDto.City
        };
    }

    /// <inheritdoc/>
    public void UpdateLocationEntity(Location location, LocationUpdateDto locationUpdateDto)
    {
        if (locationUpdateDto.City is not null)
        {
            location.City = locationUpdateDto.City;
        }
    }
}