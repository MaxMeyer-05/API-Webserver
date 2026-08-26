namespace GroceryStore.Features.Orders.Interfaces;

/// <summary>
/// Defines the contract for a service that manages orders in the grocery store application.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Retrieves all orders for a specific user from the database.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>A collection of order DTOs.</returns>
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync(Guid customerId);

    /// <summary>
    /// Retrieves a specific order by its ID from the database.
    /// </summary>
    /// <param name="orderNum">The user-specific order number.</param>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>The order DTO if found; otherwise, null.</returns>
    Task<OrderDto?> GetOrderByIdAsync(int orderNum, Guid customerId);

    /// <summary>
    /// Creates a new order in the database.
    /// </summary>
    /// <param name="order">The order DTO containing the data for the new order.</param>
    /// <param name="totalAmount">The calculated total amount of the order.</param>
    /// <returns>The created order DTO.</returns>
    Task<OrderDto> CreateOrderAsync(OrderCreateDto order);

    /// <summary>
    /// Updates an existing order in the database.
    /// </summary>
    /// <param name="orderNum">The user-specific order number.</param>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <param name="order">The order DTO containing the updated data.</param>
    Task UpdateOrderAsync(int orderNum, Guid customerId, OrderUpdateDto order);
}