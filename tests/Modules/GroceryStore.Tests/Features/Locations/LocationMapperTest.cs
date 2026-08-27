using GroceryStore.Features.Locations;
using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Locations;

[Trait("Category", "Unit")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Locations")]
public class LocationMapperTest
{
    private readonly LocationMapper _mapper = new();

    #region ToLocationDto Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToLocationDto_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("20095", "Hamburg");

        // Act
        var dto = _mapper.ToLocationDto(location);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("20095", dto.ZipCode);
        Assert.Equal("Hamburg", dto.City);
    }

    #endregion

    #region ToLocationEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToLocationEntity_ShouldMapCreateDtoToEntity()
    {
        // Arrange
        var createDto = LocationTestData.CreateLocationCreateDto("80331", "München");

        // Act
        var entity = _mapper.ToLocationEntity(createDto);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("80331", entity.ZipCode);
        Assert.Equal("München", entity.City);
    }

    #endregion

    #region UpdateLocationEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateLocationEntity_ShouldUpdateCity_WhenCityIsProvided()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("50667", "Alte Stadt");
        var updateDto = LocationTestData.CreateLocationUpdateDto("Köln");

        // Act
        _mapper.UpdateLocationEntity(location, updateDto);

        // Assert
        Assert.Equal("Köln", location.City);
        Assert.Equal("50667", location.ZipCode);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateLocationEntity_ShouldPreserveExistingCity_WhenCityIsNull()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("50667", "Köln");
        var updateDto = new LocationUpdateDto(null);

        // Act
        _mapper.UpdateLocationEntity(location, updateDto);

        // Assert
        Assert.Equal("Köln", location.City);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateLocationEntity_ShouldApplyEmptyCity_WhenProvided()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("50667", "Köln");
        var updateDto = new LocationUpdateDto(string.Empty);

        // Act
        _mapper.UpdateLocationEntity(location, updateDto);

        // Assert
        Assert.Equal(string.Empty, location.City);
    }

    #endregion
}