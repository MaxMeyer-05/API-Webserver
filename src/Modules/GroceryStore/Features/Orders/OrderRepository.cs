using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;

using GroceryStore.Features.Orders.Interfaces;
namespace GroceryStore.Features.Orders;

/// <summary>
/// Represents a repository for managing orders in the database.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly IOrderMapper _orderMapper;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(
        GroceryStoreDbContext dbContext,
        IOrderMapper orderMapper,
        ILogger<OrderRepository> logger)
    {
        _dbContext = dbContext;
        _orderMapper = orderMapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<OrderDto> CreateOrderAsync(OrderCreateDto order, decimal totalAmount)
    {
        var userOrderNumber = await GetNextUserOrderNumberAsync(order.UserId);
        var orderEntity = _orderMapper.ToOrderEntity(order, userOrderNumber, totalAmount);

        _dbContext.Orders.Add(orderEntity);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new order with Id {OrderId}", orderEntity.Id);

        return await GetOrderByIdAsync(userOrderNumber, order.UserId)
            ?? throw new InvalidOperationException($"Created order with ID {orderEntity.Id} could not be loaded");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(Guid userId)
    {
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.Ingredient)
                    .ThenInclude(ingredient => ingredient.Supplier)
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.Ingredient)
                    .ThenInclude(ingredient => ingredient.Name)
            .Where(o => o.UserId == userId)
            .ToListAsync();

        return orders.Select(o => _orderMapper.ToOrderDto(o));
    }

    /// <inheritdoc />
    public async Task<OrderDto?> GetOrderByIdAsync(int orderNum, Guid userId)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(item => item.Ingredient)
                    .ThenInclude(ingredient => ingredient.Supplier)
            .Include(o => o.OrderItems)
                .ThenInclude(item => item.Ingredient)
            .Where(o => o.UserOrderNumber == orderNum && o.UserId == userId)
            .FirstOrDefaultAsync()        
            ?? throw new KeyNotFoundException($"Order with UserOrderNumber {orderNum} not found");

        return _orderMapper.ToOrderDto(order);
    }

    /// <inheritdoc />
    public async Task UpdateOrderAsync(int orderNum, Guid userId, OrderUpdateDto order)
    {
        var orderEntity = await _dbContext.Orders
            .Where(o => o.UserOrderNumber == orderNum && o.UserId == userId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Order with UserOrderNumber {orderNum} not found");

        _orderMapper.UpdateOrderEntity(orderEntity, order);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Gets the next user order number for a given user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The next user-specific order number.</returns>
    private async Task<int> GetNextUserOrderNumberAsync(Guid userId)
    {
        var lastOrder = await _dbContext.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.UserOrderNumber)
            .FirstOrDefaultAsync();

        return lastOrder?.UserOrderNumber + 1 ?? 1;
    }
}