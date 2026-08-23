namespace GroceryStore.Features.Orders.Interfaces;

/// <summary>
/// Defines the contract for a repository that manages orders in the database.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Retrieves all orders for a specific user from the database.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of order DTOs.</returns>
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync(Guid userId);

    /// <summary>
    /// Retrieves a specific order by its ID from the database.
    /// </summary>
    /// <param name="orderNum">The user-specific order number.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The order DTO if found; otherwise, null.</returns>
    Task<OrderDto?> GetOrderByIdAsync(int orderNum, Guid userId);

    /// <summary>
    /// Creates a new order in the database.
    /// </summary>
    /// <param name="order">The order DTO containing the data for the new order.</param>
    /// <param name="totalAmount">The calculated total amount of the order.</param>
    /// <returns>The created order DTO.</returns>
    Task<OrderDto> CreateOrderAsync(OrderCreateDto order, decimal totalAmount);

    /// <summary>
    /// Updates an existing order in the database.
    /// </summary>
    /// <param name="orderNum">The user-specific order number.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="order">The order DTO containing the updated data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateOrderAsync(int orderNum, Guid userId, OrderUpdateDto order);
}