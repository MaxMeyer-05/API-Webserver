using GroceryStore.Features.Ingredients;

namespace GroceryStore.Features.Orders;

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
    int UserOrderNumber,
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
/// <param name="Ingredients">Ingredients added directly to the order.</param>
/// <param name="Recipes">Recipes whose ingredients should be added to the order.</param>
public record OrderCreateDto(
    Guid UserId,
    List<OrderItemCreateDto>? Ingredients = null,
    List<RecipeOrderItemCreateDto>? Recipes = null
);

/// <summary>
/// Represents a recipe and quantity included in an order.
/// </summary>
public record RecipeOrderItemCreateDto(
    int RecipeId,
    decimal Quantity
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
/// <param name="Ingredient">The ingredient associated with the order item.</param>
/// <param name="Quantity">The quantity of the ingredient in the order item.</param>
public record OrderItemDto(
    IngredientDto Ingredient,
    decimal Quantity
);

/// <summary>
/// Represents a data transfer object (DTO) for creating a new order item.
/// </summary>
/// <param name="IngredientId">The identifier of the ingredient associated with the order item.</param>
/// <param name="Quantity">The quantity of the ingredient in the order item.</param>
public record OrderItemCreateDto(
    int IngredientId,
    decimal Quantity
);
