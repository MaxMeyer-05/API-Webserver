using GroceryStore.Models;
using GroceryStore.Database.Entities;
using GroceryStore.Mappers.Interfaces;

namespace GroceryStore.Mappers;

/// <summary>
/// Mapper class for converting between Location entities and DTOs.
/// </summary>
public class LocationMapper : ILocationMapper
{
    /// <inheritdoc/>
    public LocationDto ToLocationDto(Location location)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Location ToLocationEntity(LocationCreateDto locationDto)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void UpdateLocationEntity(Location location, LocationUpdateDto locationUpdateDto)
    {
        throw new NotImplementedException();
    }
}