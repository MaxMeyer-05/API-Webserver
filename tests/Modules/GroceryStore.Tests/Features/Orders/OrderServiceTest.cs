using Moq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Ingredients;
using GroceryStore.Features.Ingredients.Interfaces;

using GroceryStore.Features.Orders;
using GroceryStore.Features.Orders.Interfaces;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Orders;

[Trait("Category", "Service")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Orders")]
public class OrderServiceTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<IOrderMapper> _mapperMock;
    private readonly Mock<IIngredientService> _ingredientServiceMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly OrderService _service;

    public OrderServiceTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;

        _mapperMock = new Mock<IOrderMapper>(MockBehavior.Strict);
        _ingredientServiceMock = new Mock<IIngredientService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<OrderService>>();

        _service = new OrderService(_context, _mapperMock.Object, _ingredientServiceMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region CreateOrderAsync Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateOrderAsync_ShouldCalculateTotalAmountAndSaveOrder()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var user = GroceryStoreTestData.CreateUser(zipCode: location.ZipCode);
        var ingredientEntity = GroceryStoreTestData.CreateIngredient(supplier.Id, "Milch", 1.50m);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Users.Add(user);
        _context.Ingredients.Add(ingredientEntity);
        await _context.SaveChangesAsync();

        var createDto = new OrderCreateDto(user.Id, [new OrderItemCreateDto(ingredientEntity.Id, 2)]);
        var orderEntity = new Order
        {
            UserId = user.Id,
            OrderDate = DateTime.UtcNow,
            OrderItems = [new OrderItem { IngredientId = ingredientEntity.Id, Quantity = 2 }]
        };

        var ingredientDto = new IngredientDto(
            IngredientId: ingredientEntity.Id,
            SupplierIngredientCount: 0,
            Name: "Milch",
            Unit: "Liter",
            NetPrice: 1.50m,
            Stock: 50,
            SupplierId: supplier.Id,
            SupplierName: supplier.CompanyName,
            Calories: null,
            Carbohydrates: null,
            Protein: null);

        var expectedDto = OrderTestData.CreateOrderDto(1, user.Id, 3.00m);

        _mapperMock.Setup(m => m.ToOrderEntity(createDto)).Returns(orderEntity);
        _ingredientServiceMock.Setup(s => s.GetIngredientByIdAsync(ingredientEntity.Id)).ReturnsAsync(ingredientDto);
        _mapperMock.Setup(m => m.ToOrderDto(It.IsAny<Order>())).Returns(expectedDto);

        // Act
        var result = await _service.CreateOrderAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(3.00m, result.TotalAmount);

        var persistedOrder = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.UserId == user.Id);
        Assert.NotNull(persistedOrder);
        Assert.Single(persistedOrder.OrderItems);
    }

    #endregion

    #region GetAllOrdersAsync & GetOrderByIdAsync Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllOrdersAsync_ShouldReturnOrdersOnlyForSpecifiedUser()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user1 = GroceryStoreTestData.CreateUser(email: "user1@example.com", zipCode: location.ZipCode);
        var user2 = GroceryStoreTestData.CreateUser(email: "user2@example.com", zipCode: location.ZipCode);

        var order1 = OrderTestData.CreateOrder(userId: user1.Id);
        var order2 = OrderTestData.CreateOrder(userId: user2.Id);

        _context.Locations.Add(location);
        _context.Users.AddRange(user1, user2);
        _context.Orders.AddRange(order1, order2);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.ToOrderDto(It.Is<Order>(o => o.UserId == user1.Id)))
            .Returns(OrderTestData.CreateOrderDto(1, user1.Id));

        // Act
        var result = await _service.GetAllOrdersAsync(user1.Id);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        var singleOrder = Assert.Single(list);
        Assert.Equal(user1.Id, singleOrder.UserId);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetOrderByIdAsync_ShouldReturnMappedDto_WhenOrderBelongsToUser()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = GroceryStoreTestData.CreateUser(zipCode: location.ZipCode);
        var order = OrderTestData.CreateOrder(userId: user.Id);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var expectedDto = OrderTestData.CreateOrderDto(1, user.Id);
        _mapperMock.Setup(m => m.ToOrderDto(It.Is<Order>(o => o.Id == order.Id))).Returns(expectedDto);

        // Act
        var result = await _service.GetOrderByIdAsync(order.Id, user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetOrderByIdAsync_ShouldThrowKeyNotFoundException_WhenOrderDoesNotExistOrBelongsToAnotherUser()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user1 = GroceryStoreTestData.CreateUser(email: "u1@test.com", zipCode: location.ZipCode);
        var user2 = GroceryStoreTestData.CreateUser(email: "u2@test.com", zipCode: location.ZipCode);
        var order = OrderTestData.CreateOrder(userId: user1.Id);

        _context.Locations.Add(location);
        _context.Users.AddRange(user1, user2);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.GetOrderByIdAsync(order.Id, user2.Id));
    }

    #endregion

    #region UpdateOrderAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateOrderAsync_ShouldApplyUpdates_WhenOrderIsOpenAndBelongsToUser()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = GroceryStoreTestData.CreateUser(zipCode: location.ZipCode);
        var order = OrderTestData.CreateOrder(userId: user.Id, isCanceled: false, isCompleted: false);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var updateDto = new OrderUpdateDto(IsCanceled: true, IsCompleted: null);
        _mapperMock.Setup(m => m.UpdateOrderEntity(order, updateDto))
            .Callback<Order, OrderUpdateDto>((entity, dto) => entity.IsCanceled = dto.IsCanceled!.Value);

        // Act
        await _service.UpdateOrderAsync(order.Id, user.Id, updateDto);

        // Assert
        var updated = await _context.Orders.FindAsync(order.Id);
        Assert.NotNull(updated);
        Assert.True(updated.IsCanceled);
        _mapperMock.Verify(m => m.UpdateOrderEntity(order, updateDto), Times.Once);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateOrderAsync_ShouldThrowInvalidOperationException_WhenOrderIsAlreadyCanceled()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = GroceryStoreTestData.CreateUser(zipCode: location.ZipCode);
        var order = OrderTestData.CreateOrder(userId: user.Id, isCanceled: true);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var updateDto = new OrderUpdateDto(IsCanceled: false, IsCompleted: true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateOrderAsync(order.Id, user.Id, updateDto));
        Assert.Contains("already canceled", ex.Message);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateOrderAsync_ShouldThrowInvalidOperationException_WhenOrderIsAlreadyCompleted()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = GroceryStoreTestData.CreateUser(zipCode: location.ZipCode);
        var order = OrderTestData.CreateOrder(userId: user.Id, isCompleted: true);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var updateDto = new OrderUpdateDto(IsCanceled: true, IsCompleted: null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateOrderAsync(order.Id, user.Id, updateDto));
        Assert.Contains("already completed", ex.Message);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateOrderAsync_ShouldThrowKeyNotFoundException_WhenOrderNotFound()
    {
        // Arrange
        var updateDto = new OrderUpdateDto(IsCanceled: true, IsCompleted: null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateOrderAsync(999, Guid.NewGuid(), updateDto));
    }

    #endregion
}