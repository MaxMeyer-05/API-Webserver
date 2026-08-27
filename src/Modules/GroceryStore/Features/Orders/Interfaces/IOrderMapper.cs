using GroceryStore.Database.Entities;
namespace GroceryStore.Features.Orders.Interfaces;

/// <summary>
/// Defines the contract for mapping between Order entities and DTOs.
/// </summary>
public interface IOrderMapper
{
    /// <summary>
    /// Maps an <see cref="OrderCreateDto"/> to an <see cref="Order"/> entity.
    /// </summary>
    /// <param name="orderCreateDto">The <see cref="OrderCreateDto"/> to map.</param>
    /// <returns>The mapped <see cref="Order"/> entity.</returns>
    Order ToOrderEntity(OrderCreateDto orderCreateDto);

    /// <summary>
    /// Maps an <see cref="Order"/> entity to an <see cref="OrderDto"/>.
    /// </summary>
    /// <param name="orderEntity">The <see cref="Order"/> entity to map.</param>
    /// <returns>The mapped <see cref="OrderDto"/>.</returns>
    OrderDto ToOrderDto(Order orderEntity);

    /// <summary>
    /// Updates an existing <see cref="Order"/> entity with values from an <see cref="OrderUpdateDto"/>.
    /// </summary>
    /// <param name="orderEntity">The <see cref="Order"/> entity to update.</param>
    /// <param name="orderUpdateDto">The <see cref="OrderUpdateDto"/> containing updated values.</param>
    void UpdateOrderEntity(Order orderEntity, OrderUpdateDto orderUpdateDto);


    /// <summary>
    /// Maps an <see cref="OrderItemCreateDto"/> to an <see cref="OrderItem"/> entity.
    /// </summary>
    /// <param name="orderDto">The <see cref="OrderItemCreateDto"/> to map.</param>
    /// <returns>The mapped <see cref="OrderItem"/> entity.</returns>
    OrderItem ToOrderItemEntity(OrderItemCreateDto orderDto);

    /// <summary>
    /// Maps an <see cref="OrderItem"/> entity to an <see cref="OrderItemDto"/>.
    /// </summary>
    /// <param name="orderItemEntity">The <see cref="OrderItem"/> entity to map.</param>
    /// <returns>The mapped <see cref="OrderItemDto"/>.</returns>
    OrderItemDto ToOrderItemDto(OrderItem orderItemEntity);
}