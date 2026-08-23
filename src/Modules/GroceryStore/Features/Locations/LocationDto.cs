namespace GroceryStore.Features.Locations;

/// <summary>
/// Represents a data transfer object (DTO) for a location.
/// </summary>
/// <param name="ZipCode">The postal code of the location.</param>
/// <param name="City">The city name of the location.</param>
public record LocationDto(
    string ZipCode,
    string City
);

/// <summary>
/// Represents the data required to create a new location.
/// </summary>
/// <param name="ZipCode">The postal code of the location.</param>
/// <param name="City">The city name of the location.</param>
public record LocationCreateDto(
    string ZipCode,
    string City
);

/// <summary>
/// Represents the data required to update an existing location.
/// </summary>
/// <param name="City">The city name of the location.</param>
public record LocationUpdateDto(
    string? City
);