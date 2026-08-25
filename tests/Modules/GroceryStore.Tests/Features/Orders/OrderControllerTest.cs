using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Ingredients.Interfaces;

using GroceryStore.Features.Orders;
using GroceryStore.Features.Orders.Interfaces;

using GroceryStore.Tests.TestData;
using SharedKernel.Security.Interfaces;

namespace GroceryStore.Tests.Features.Orders;

[Trait("Category", "Controller")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Orders")]
public class OrderControllerTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<IOrderMapper> _mapperMock;
    private readonly Mock<IIngredientService> _ingredientServiceMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly OrderService _service;
    private readonly OrderController _controller;

    public OrderControllerTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;

        _mapperMock = new Mock<IOrderMapper>(MockBehavior.Strict);
        _ingredientServiceMock = new Mock<IIngredientService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<OrderService>>();
        _currentUserMock = new Mock<ICurrentUser>(MockBehavior.Strict);

        _service = new OrderService(_context, _mapperMock.Object, _ingredientServiceMock.Object, _loggerMock.Object);
        _controller = new OrderController(_service, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetAllOrders Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllOrders_ShouldReturnOkWithUserOrders()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = GroceryStoreTestData.CreateUser(zipCode: location.ZipCode);
        var order = OrderTestData.CreateOrder(userId: user.Id);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(user.Id);

        var expectedDto = OrderTestData.CreateOrderDto(1, user.Id);
        _mapperMock.Setup(m => m.ToOrderDto(It.IsAny<Order>())).Returns(expectedDto);

        // Act
        var result = await _controller.GetAllOrders();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<OrderDto>>(okResult.Value);
        Assert.Single(returnedItems);
    }

    #endregion

    #region GetOrderById Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetOrderById_ShouldReturnOk_WhenOrderExistsForUser()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = GroceryStoreTestData.CreateUser(zipCode: location.ZipCode);
        var order = OrderTestData.CreateOrder(userId: user.Id);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(user.Id);

        var expectedDto = OrderTestData.CreateOrderDto(1, user.Id);
        _mapperMock.Setup(m => m.ToOrderDto(It.Is<Order>(o => o.Id == order.Id))).Returns(expectedDto);

        // Act
        var result = await _controller.GetOrderById(order.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedDto, okResult.Value);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetOrderById_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _controller.GetOrderById(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region CreateOrder Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateOrder_ShouldReturnCreatedAtAction_WhenValid()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = GroceryStoreTestData.CreateUser(zipCode: location.ZipCode);
        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var createDto = new OrderCreateDto(user.Id, []);
        var entity = new Order { UserId = user.Id, OrderDate = DateTime.UtcNow };
        var createdDto = OrderTestData.CreateOrderDto(1, user.Id);

        _mapperMock.Setup(m => m.ToOrderEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToOrderDto(It.IsAny<Order>())).Returns(createdDto);

        // Act
        var result = await _controller.CreateOrder(createDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, createdAtResult.StatusCode);
        Assert.Equal(nameof(OrderController.GetOrderById), createdAtResult.ActionName);
        Assert.Same(createdDto, createdAtResult.Value);
    }

    #endregion

    #region UpdateOrder Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateOrder_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = GroceryStoreTestData.CreateUser(zipCode: location.ZipCode);
        var order = OrderTestData.CreateOrder(userId: user.Id, isCanceled: false, isCompleted: false);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(user.Id);
        var updateDto = new OrderUpdateDto(IsCanceled: true, IsCompleted: null);
        _mapperMock.Setup(m => m.UpdateOrderEntity(order, updateDto))
            .Callback<Order, OrderUpdateDto>((e, dto) => e.IsCanceled = dto.IsCanceled!.Value);

        // Act
        var result = await _controller.UpdateOrder(order.Id, updateDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateOrder_ShouldReturnBadRequest_WhenOrderAlreadyClosed()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = GroceryStoreTestData.CreateUser(zipCode: location.ZipCode);
        var order = OrderTestData.CreateOrder(userId: user.Id, isCanceled: true);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(user.Id);
        var updateDto = new OrderUpdateDto(IsCanceled: false, IsCompleted: true);

        // Act
        var result = await _controller.UpdateOrder(order.Id, updateDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateOrder_ShouldReturnNotFound_WhenOrderNotFound()
    {
        // Arrange
        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        var updateDto = new OrderUpdateDto(IsCanceled: true, IsCompleted: null);

        // Act
        var result = await _controller.UpdateOrder(999, updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion
}