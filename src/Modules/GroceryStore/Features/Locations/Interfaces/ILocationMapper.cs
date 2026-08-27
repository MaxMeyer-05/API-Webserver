using GroceryStore.Database.Entities;

namespace GroceryStore.Features.Locations.Interfaces;
public interface ILocationMapper
{
    /// <summary>
    /// Maps a <see cref="LocationCreateDto"/> to a <see cref="Location"/> entity.
    /// </summary>
    /// <param name="locationDto">The <see cref="LocationCreateDto"/> to map.</param>
    /// <returns>The mapped <see cref="Location"/> entity.</returns>
    Location ToLocationEntity(LocationCreateDto locationDto);
    
    /// <summary>
    /// Maps a <see cref="Location"/> entity to a <see cref="LocationDto"/>.
    /// </summary>
    /// <param name="location">The <see cref="Location"/> entity to map.</param>
    /// <returns>The mapped <see cref="LocationDto"/>.</returns>
    LocationDto ToLocationDto(Location location);

    /// <summary>
    /// Updates an existing <see cref="Location"/> entity with values from a <see cref="LocationUpdateDto"/>.
    /// </summary>
    /// <param name="location">The <see cref="Location"/> entity to update.</param>
    /// <param name="locationUpdateDto">The <see cref="LocationUpdateDto"/> containing updated values.</param>
    void UpdateLocationEntity(Location location, LocationUpdateDto locationUpdateDto);
}