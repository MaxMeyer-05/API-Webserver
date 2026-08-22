using GroceryStore.Models;
using GroceryStore.Database.Entities;
using GroceryStore.Mappers.Interfaces;

namespace GroceryStore.Mappers;

/// <summary>
/// Mapper class for converting between Order entities and DTOs.
/// </summary>
public class OrderMapper : IOrderMapper
{
    /// <inheritdoc/>
    public OrderDto ToOrderDto(Order orderEntity)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Order ToOrderEntity(OrderCreateDto orderCreateDto)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Order ToOrderEntity(OrderCreateDto orderCreateDto, RecipeDto orderItems)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public OrderItemDto ToOrderItemDto(OrderItem orderItemEntity)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Order ToOrderItemEntity(OrderCreateDto orderDto)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void UpdateOrderEntity(Order orderEntity, OrderUpdateDto orderUpdateDto)
    {
        throw new NotImplementedException();
    }
}