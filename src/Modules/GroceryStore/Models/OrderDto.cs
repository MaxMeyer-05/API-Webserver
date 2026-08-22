namespace GroceryStore.Models;

/// <summary>
/// Represents a data transfer object (DTO) for an order.
/// </summary>
/// <param name="UserId">The identifier of the user who placed the order.</param>
/// <param name="OrderDate">The date and time when the order was placed.</param>
/// <param name="TotalAmount">The total amount of the order.</param>
/// <param name="IsCanceled">Indicates whether the order has been canceled.</param>
/// <param name="IsCompleted">Indicates whether the order has been completed.</param>
/// <param name="Items">A list of order item DTOs associated with the order.</param>
public record OrderDto(
    Guid UserId,
    DateTime OrderDate,
    decimal TotalAmount,
    bool IsCanceled,
    bool IsCompleted,
    List<OrderItemDto> Items
);

/// <summary>
/// Represents a data transfer object (DTO) for creating a new order.
/// </summary>
/// <param name="UserId">The identifier of the user who placed the order.</param>
/// <param name="TotalAmount">The total amount of the order.</param>
/// <param name="Items">A list of order item DTOs associated with the order.</param>
public record OrderCreateDto(
    Guid UserId,
    decimal TotalAmount,
    List<OrderItemCreateDto> Items
);

/// <summary>
/// Represents a data transfer object (DTO) for updating an existing order.
/// </summary>
/// <param name="IsCanceled">Indicates whether the order has been canceled.</param>
/// <param name="IsCompleted">Indicates whether the order has been completed.</param>
public record OrderUpdateDto(
    bool? IsCanceled,
    bool? IsCompleted
);


/// <summary>
/// Represents a data transfer object (DTO) for an order item.
/// </summary>
/// <param name="Order">The identifier of the order to which the item belongs.</param>
/// <param name="Ingredient">The ingredient associated with the order item.</param>
/// <param name="Quantity">The quantity of the ingredient in the order item.</param>
public record OrderItemDto(
    OrderDto Order,
    IngredientDto Ingredient,
    int Quantity
);

/// <summary>
/// Represents a data transfer object (DTO) for creating a new order item.
/// </summary>
/// <param name="OrderId">The identifier of the order to which the item belongs.</param>
/// <param name="IngredientId">The identifier of the ingredient associated with the order item.</param>
/// <param name="Quantity">The quantity of the ingredient in the order item.</param>
public record OrderItemCreateDto(
    int OrderId,
    int IngredientId,
    int Quantity
);
