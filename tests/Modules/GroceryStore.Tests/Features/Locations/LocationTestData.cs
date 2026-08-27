using GroceryStore.Features.Locations;

namespace GroceryStore.Tests.Features.Locations;

public static class LocationTestData
{
    #region DTO Fixtures

    public static LocationDto CreateLocationDto(
        string zipCode = "10115",
        string city = "Berlin") => new(
        ZipCode: zipCode,
        City: city);

    public static LocationCreateDto CreateLocationCreateDto(
        string zipCode = "80331",
        string city = "München") => new(
        ZipCode: zipCode,
        City: city);

    public static LocationUpdateDto CreateLocationUpdateDto(
        string? city = "Hamburg") => new(
        City: city);

    #endregion
}