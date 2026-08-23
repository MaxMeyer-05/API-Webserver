using GroceryStore.Database.Entities;

using GroceryStore.Features.Recipes;
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
            UserId: orderEntity.UserId,
            OrderDate: orderEntity.OrderDate,
            TotalAmount: orderEntity.TotalAmount,
            IsCanceled: orderEntity.IsCanceled,
            IsCompleted: orderEntity.IsCompleted,
            Items: orderEntity.OrderItems?.Select(ToOrderItemDto).ToList() ?? []
        );
    }

    /// <inheritdoc/>
    public Order ToOrderEntity(OrderCreateDto orderCreateDto)
    {
        return new Order
        {
            UserId = orderCreateDto.UserId,
            TotalAmount = orderCreateDto.TotalAmount,
            OrderItems = orderCreateDto.Items?.Select(i => new OrderItem
            {
                OrderId = i.OrderId,
                IngredientId = i.IngredientId,
                Quantity = i.Quantity
            }).ToList() ?? []
        };
    }

    /// <inheritdoc/>
    public Order ToOrderEntity(OrderCreateDto orderCreateDto, RecipeDto orderItems)
    {
        var order = ToOrderEntity(orderCreateDto);
        decimal totalAmount = 0;

        foreach (var item in orderItems.Ingredients!)
        {
            var itemPrice = item.Ingredient.NetPrice;
            var itemAmount = item.Amount;

            order.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                IngredientId = item.Ingredient.IngredientId,
                Quantity = (int)itemAmount
            });

            totalAmount += itemPrice * itemAmount;
        }

        order.TotalAmount = totalAmount;
        return order;
    }

    /// <inheritdoc/>
    public OrderItemDto ToOrderItemDto(OrderItem orderItemEntity)
    {
        return new OrderItemDto(
            Order: ToOrderDto(orderItemEntity.Order),
            Ingredient: _ingredientMapper.ToIngredientDto(orderItemEntity.Ingredient),
            Quantity: orderItemEntity.Quantity
        );
    }

    /// <inheritdoc/>
    public OrderItem ToOrderItemEntity(OrderItemCreateDto orderDto)
    {
        return new OrderItem
        {
            OrderId = orderDto.OrderId,
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