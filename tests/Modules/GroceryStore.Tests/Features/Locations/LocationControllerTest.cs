using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Locations;
using GroceryStore.Features.Locations.Interfaces;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Locations;

[Trait("Category", "Controller")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Locations")]
public class LocationControllerTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<ILocationMapper> _mapperMock;
    private readonly Mock<ILogger<LocationService>> _loggerMock;
    private readonly LocationService _service;
    private readonly LocationController _controller;

    public LocationControllerTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;

        _mapperMock = new Mock<ILocationMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<LocationService>>();

        _service = new LocationService(_context, _mapperMock.Object, _loggerMock.Object);
        _controller = new LocationController(_service);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetAllLocations Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllLocations_ShouldReturnOkWithLocations()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("10115", "Berlin");
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var expectedDto = LocationTestData.CreateLocationDto("10115", "Berlin");
        _mapperMock.Setup(m => m.ToLocationDto(It.IsAny<Location>())).Returns(expectedDto);

        // Act
        var result = await _controller.GetAllLocations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var returnedList = Assert.IsAssignableFrom<IEnumerable<LocationDto>>(okResult.Value);
        Assert.Single(returnedList);
    }

    #endregion

    #region GetLocationByZipCode Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetLocationByZipCode_ShouldReturnOk_WhenLocationExists()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("20095", "Hamburg");
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var expectedDto = LocationTestData.CreateLocationDto("20095", "Hamburg");
        _mapperMock.Setup(m => m.ToLocationDto(It.Is<Location>(l => l.ZipCode == "20095"))).Returns(expectedDto);

        // Act
        var result = await _controller.GetLocationByZipCode("20095");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedDto, okResult.Value);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetLocationByZipCode_ShouldReturnNotFound_WhenLocationDoesNotExist()
    {
        // Act
        var result = await _controller.GetLocationByZipCode("99999");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region CreateLocation Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateLocation_ShouldReturnCreatedAtAction_WhenValid()
    {
        // Arrange
        var createDto = LocationTestData.CreateLocationCreateDto("80331", "München");
        var entity = new Location { ZipCode = "80331", City = "München" };
        var createdDto = LocationTestData.CreateLocationDto("80331", "München");

        _mapperMock.Setup(m => m.ToLocationEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToLocationDto(It.Is<Location>(l => l.ZipCode == "80331"))).Returns(createdDto);

        // Act
        var result = await _controller.CreateLocation(createDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, createdAtResult.StatusCode);
        Assert.Equal(nameof(LocationController.GetLocationByZipCode), createdAtResult.ActionName);
        Assert.Same(createdDto, createdAtResult.Value);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateLocation_ShouldReturnBadRequest_WhenMappingFails()
    {
        // Arrange
        var createDto = LocationTestData.CreateLocationCreateDto();
        var entity = new Location { ZipCode = createDto.ZipCode, City = createDto.City };

        _mapperMock.Setup(m => m.ToLocationEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToLocationDto(It.IsAny<Location>())).Returns((LocationDto)null!);

        // Act
        var result = await _controller.CreateLocation(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    #endregion

    #region UpdateLocation Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateLocation_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("50667", "Köln");
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var updateDto = LocationTestData.CreateLocationUpdateDto("Köln am Rhein");
        _mapperMock.Setup(m => m.UpdateLocationEntity(location, updateDto))
            .Callback<Location, LocationUpdateDto>((entity, dto) => entity.City = dto.City!);

        // Act
        var result = await _controller.UpdateLocation("50667", updateDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateLocation_ShouldReturnNotFound_WhenLocationDoesNotExist()
    {
        // Arrange
        var updateDto = LocationTestData.CreateLocationUpdateDto();

        // Act
        var result = await _controller.UpdateLocation("99999", updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region DeleteLocation Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteLocation_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("28195", "Bremen");
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteLocation("28195");

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteLocation_ShouldReturnNotFound_WhenLocationDoesNotExist()
    {
        // Act
        var result = await _controller.DeleteLocation("99999");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion
}