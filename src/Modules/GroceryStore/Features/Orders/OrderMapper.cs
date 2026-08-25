using GroceryStore.Database.Entities;

using GroceryStore.Features.Orders.Interfaces;
using GroceryStore.Features.Ingredients.Interfaces;

namespace GroceryStore.Features.Orders;

/// <summary>
/// Mapper class for converting between Order entities and DTOs.
/// </summary>
public class OrderMapper : IOrderMapper
{
    private readonly IIngredientMapper _ingredientMapper;

    public OrderMapper(IIngredientMapper ingredientMapper)
    {
        _ingredientMapper = ingredientMapper;
    }

    /// <inheritdoc/>
    public OrderDto ToOrderDto(Order orderEntity)
    {
        return new OrderDto(
            UserOrderNumber: orderEntity.UserOrderNumber,
            UserId: orderEntity.UserId,
            OrderDate: orderEntity.OrderDate,
            TotalAmount: orderEntity.TotalAmount,
            IsCanceled: orderEntity.IsCanceled,
            IsCompleted: orderEntity.IsCompleted,
            Items: orderEntity.OrderItems?.Select(ToOrderItemDto).ToList() ?? []
        );
    }

    /// <inheritdoc/>
    public Order ToOrderEntity(OrderCreateDto orderCreateDto, int userOrderNumber)
    {
        return new Order
        {
            UserOrderNumber = userOrderNumber,
            UserId = orderCreateDto.UserId,
            OrderDate = DateTime.UtcNow,
            OrderItems = orderCreateDto.Ingredients?.Select(i => new OrderItem
            {
                IngredientId = i.IngredientId,
                Quantity = i.Quantity
            }).ToList() ?? []
        };
    }

    /// <inheritdoc/>
    public OrderItemDto ToOrderItemDto(OrderItem orderItemEntity)
    {
        return new OrderItemDto(
            Ingredient: _ingredientMapper.ToIngredientDto(orderItemEntity.Ingredient),
            Quantity: orderItemEntity.Quantity
        );
    }

    /// <inheritdoc/>
    public OrderItem ToOrderItemEntity(OrderItemCreateDto orderDto)
    {
        return new OrderItem
        {
            IngredientId = orderDto.IngredientId,
            Quantity = orderDto.Quantity
        };
    }

    /// <inheritdoc/>
    public void UpdateOrderEntity(Order orderEntity, OrderUpdateDto orderUpdateDto)
    {
        if (orderUpdateDto.IsCanceled is not null)
        {
            orderEntity.IsCanceled = orderUpdateDto.IsCanceled.Value;
        }

        if (orderUpdateDto.IsCompleted is not null)
        {
            orderEntity.IsCompleted = orderUpdateDto.IsCompleted.Value;
        }
    }
}