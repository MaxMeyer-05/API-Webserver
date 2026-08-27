using Moq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Locations;
using GroceryStore.Features.Locations.Interfaces;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Locations;

[Trait("Category", "Service")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Locations")]
public class LocationServiceTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<ILocationMapper> _mapperMock;
    private readonly Mock<ILogger<LocationService>> _loggerMock;
    private readonly LocationService _service;

    public LocationServiceTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;
        _mapperMock = new Mock<ILocationMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<LocationService>>();

        _service = new LocationService(_context, _mapperMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region CreateLocationAsync Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateLocationAsync_ShouldPersistLocationAndReturnDto()
    {
        // Arrange
        var createDto = LocationTestData.CreateLocationCreateDto("28195", "Bremen");
        var entity = new Location { ZipCode = "28195", City = "Bremen" };
        var expectedDto = LocationTestData.CreateLocationDto("28195", "Bremen");

        _mapperMock.Setup(m => m.ToLocationEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToLocationDto(It.Is<Location>(l => l.ZipCode == "28195"))).Returns(expectedDto);

        // Act
        var result = await _service.CreateLocationAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("28195", result.ZipCode);
        Assert.Equal("Bremen", result.City);

        var persisted = await _context.Locations.FindAsync("28195");
        Assert.NotNull(persisted);
        Assert.Equal("Bremen", persisted.City);

        _mapperMock.Verify(m => m.ToLocationEntity(createDto), Times.Once);
        _mapperMock.Verify(m => m.ToLocationDto(entity), Times.Once);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateLocationAsync_ShouldThrowInvalidOperationException_WhenDtoMappingFails()
    {
        // Arrange
        var createDto = LocationTestData.CreateLocationCreateDto("28195", "Bremen");
        var entity = new Location { ZipCode = createDto.ZipCode, City = createDto.City };

        _mapperMock.Setup(m => m.ToLocationEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToLocationDto(entity)).Returns((LocationDto)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateLocationAsync(createDto));
        Assert.Equal("Failed to create location", ex.Message);
    }

    #endregion

    #region GetAllLocationsAsync & GetLocationByZipCodeAsync Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllLocationsAsync_ShouldReturnAllMappedLocations()
    {
        // Arrange
        await _context.Locations.ExecuteDeleteAsync();
        var loc1 = GroceryStoreTestData.CreateLocation("28195", "Bremen");
        var loc2 = GroceryStoreTestData.CreateLocation("01067", "Dresden");

        _context.Locations.AddRange(loc1, loc2);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.ToLocationDto(It.Is<Location>(l => l.ZipCode == "28195")))
            .Returns(LocationTestData.CreateLocationDto("28195", "Bremen"));
        _mapperMock.Setup(m => m.ToLocationDto(It.Is<Location>(l => l.ZipCode == "01067")))
            .Returns(LocationTestData.CreateLocationDto("01067", "Dresden"));

        // Act
        var result = await _service.GetAllLocationsAsync();

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, l => l.ZipCode == "28195");
        Assert.Contains(list, l => l.ZipCode == "01067");
        _mapperMock.Verify(m => m.ToLocationDto(loc1), Times.Once);
        _mapperMock.Verify(m => m.ToLocationDto(loc2), Times.Once);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllLocationsAsync_ShouldReturnEmptyCollection_WhenNoLocationsExist()
    {
        // Arrange
        await _context.Locations.ExecuteDeleteAsync();

        // Act
        var result = await _service.GetAllLocationsAsync();

        // Assert
        Assert.Empty(result);
        _mapperMock.Verify(m => m.ToLocationDto(It.IsAny<Location>()), Times.Never);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetLocationByZipCodeAsync_ShouldReturnMappedDto_WhenLocationExists()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("01067", "Dresden");
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var expectedDto = LocationTestData.CreateLocationDto("01067", "Dresden");
        _mapperMock.Setup(m => m.ToLocationDto(It.Is<Location>(l => l.ZipCode == "01067"))).Returns(expectedDto);

        // Act
        var result = await _service.GetLocationByZipCodeAsync("01067");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("01067", result.ZipCode);
        Assert.Equal("Dresden", result.City);
        _mapperMock.Verify(m => m.ToLocationDto(location), Times.Once);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetLocationByZipCodeAsync_ShouldThrowKeyNotFoundException_WhenLocationDoesNotExist()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetLocationByZipCodeAsync("99999"));
        Assert.Contains("99999", ex.Message);
    }

    #endregion

    #region UpdateLocationAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateLocationAsync_ShouldApplyUpdatesAndPersistChanges()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("28195", "Bremen");
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var updateDto = LocationTestData.CreateLocationUpdateDto("Frankfurt am Main");
        _mapperMock.Setup(m => m.UpdateLocationEntity(location, updateDto))
            .Callback<Location, LocationUpdateDto>((entity, dto) => entity.City = dto.City!);

        // Act
        await _service.UpdateLocationAsync("28195", updateDto);

        // Assert
        var updated = await _context.Locations.FindAsync("28195");
        Assert.NotNull(updated);
        Assert.Equal("Frankfurt am Main", updated.City);
        _mapperMock.Verify(m => m.UpdateLocationEntity(location, updateDto), Times.Once);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateLocationAsync_ShouldThrowKeyNotFoundException_WhenLocationMissing()
    {
        // Arrange
        var updateDto = LocationTestData.CreateLocationUpdateDto();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateLocationAsync("99999", updateDto));
    }

    #endregion

    #region DeleteLocationAsync Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteLocationAsync_ShouldRemoveLocation_WhenLocationExists()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("28195", "Bremen");
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteLocationAsync("28195");

        // Assert
        var exists = await _context.Locations.AnyAsync(l => l.ZipCode == "28195");
        Assert.False(exists);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteLocationAsync_ShouldThrowKeyNotFoundException_WhenLocationMissing()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.DeleteLocationAsync("99999"));
    }

    #endregion
}