using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;

using GroceryStore.Features.Ingredients.Interfaces;
using GroceryStore.Features.Orders.Interfaces;

namespace GroceryStore.Features.Orders;

/// <summary>
/// Represents a service for managing orders in the grocery store application.
/// </summary>
public class OrderService : IOrderService
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly IOrderMapper _orderMapper;
    private readonly IIngredientService _ingredientRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        GroceryStoreDbContext dbContext,
        IOrderMapper orderMapper,
        IIngredientService ingredientRepository,
        ILogger<OrderService> logger)
    {
        _dbContext = dbContext;
        _orderMapper = orderMapper;
        _ingredientRepository = ingredientRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<OrderDto> CreateOrderAsync(OrderCreateDto order)
    {
        var orderEntity = _orderMapper.ToOrderEntity(order);

        decimal totalAmount = 0;
        foreach (var item in orderEntity.OrderItems)
        {
            var ingredient = await _ingredientRepository.GetIngredientByIdAsync(item.IngredientId);
            totalAmount += ingredient!.NetPrice * item.Quantity;
        }

        _dbContext.Orders.Add(orderEntity);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new order with Id {OrderId}", orderEntity.Id);

        return await GetOrderByIdAsync(orderEntity.Id, order.UserId)
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
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<OrderDto?> GetOrderByIdAsync(int orderNum, Guid userId)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(item => item.Ingredient)
                    .ThenInclude(ingredient => ingredient.Supplier)
            .Include(o => o.OrderItems)
                .ThenInclude(item => item.Ingredient)
            .Where(o => o.Id == orderNum && o.UserId == userId)
            .FirstOrDefaultAsync()        
            ?? throw new KeyNotFoundException($"Order with ID {orderNum} not found");

        if (order.UserId != userId)
            throw new UnauthorizedAccessException($"User with ID {userId} does not have permission to access order with ID {orderNum}");

        return _orderMapper.ToOrderDto(order);
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task UpdateOrderAsync(int orderNum, Guid userId, OrderUpdateDto order)
    {
        var orderEntity = await _dbContext.Orders
            .Where(o => o.Id == orderNum && o.UserId == userId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Order with ID {orderNum} not found");

        if (orderEntity.UserId != userId)
            throw new UnauthorizedAccessException($"User with ID {userId} does not have permission to update order with ID {orderNum}");

        if (orderEntity.IsCanceled)
            throw new InvalidOperationException($"Order with ID {orderNum} is already canceled and cannot be updated");
        
        if (orderEntity.IsCompleted)
            throw new InvalidOperationException($"Order with ID {orderNum} is already completed and cannot be updated");
        
        _orderMapper.UpdateOrderEntity(orderEntity, order);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Updated order with ID {OrderNum} for user {UserId}", orderNum, userId);
    }
}