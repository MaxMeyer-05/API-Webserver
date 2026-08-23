using GroceryStore.Features.Ingredients.Interfaces;
using GroceryStore.Features.Orders.Interfaces;
using GroceryStore.Features.Recipes.Interfaces;

namespace GroceryStore.Features.Orders;

/// <summary>
/// Represents a service for managing orders,
/// providing business logic and validation for order-related operations.
/// </summary>
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IIngredientRepository ingredientRepository,
        IRecipeRepository recipeRepository,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _ingredientRepository = ingredientRepository;
        _recipeRepository = recipeRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all orders for a specific user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>Returns a collection of <see cref="OrderDto"/> representing the user's orders.</returns>
    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(Guid userId)
    {
        _logger.LogDebug("Retrieving all orders for user {UserId}", userId);
        return await _orderRepository.GetAllOrdersAsync(userId);
    }

    /// <summary>
    /// Retrieves a specific order by its ID and user ID.
    /// </summary>
    /// <param name="orderId">The identifier of the order.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>Returns the <see cref="OrderDto"/> representing the order if found; otherwise, null.</returns>
    public async Task<OrderDto?> GetOrderByIdAsync(int orderId, Guid userId)
    {
        _logger.LogDebug("Retrieving order with ID {OrderId} for user {UserId}", orderId, userId);
        return await _orderRepository.GetOrderByIdAsync(orderId, userId);
    }

    /// <summary>
    /// Updates an existing order for a specific user.
    /// </summary>
    /// <param name="orderId">The identifier of the order to update.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="order">The <see cref="OrderUpdateDto"/> containing updated values for the order.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the order with the specified ID and user ID is not found.</exception>
    public async Task UpdateOrderAsync(int orderId, Guid userId, OrderUpdateDto order)
    {
        var existingOrder = await _orderRepository.GetOrderByIdAsync(orderId, userId)
            ?? throw new KeyNotFoundException($"Order with ID {orderId} for user {userId} not found");

        if (order.IsCanceled.HasValue && order.IsCanceled.Value && existingOrder.IsCompleted)
            throw new InvalidOperationException("Cannot cancel a completed order");

        if (existingOrder.UserId != userId)
            throw new UnauthorizedAccessException("User does not have permission to update this order");

        await _orderRepository.UpdateOrderAsync(orderId, userId, order);
    }

    /// <summary>
    /// Creates a new order for a specific user.
    /// </summary>
    /// <param name="order">The <see cref="OrderCreateDto"/> containing the details of the order to create.</param>
    /// <returns>Returns the created <see cref="OrderDto"/> representing the new order.</returns>
    /// <exception cref="ArgumentException">Thrown if the order does not contain any ingredients or recipes.</exception>
    public async Task<OrderDto> CreateOrderAsync(OrderCreateDto order)
    {
        if (order.Ingredients is not { Count: > 0 } && order.Recipes is not { Count: > 0 })
            throw new ArgumentException("An order must contain at least one ingredient or recipe", nameof(order));

        var expandedIngredients = await ExpandIngredientsAsync(order);
        var mergedIngredients = MergeIngredients(expandedIngredients);
        var totalAmount = await CalculateTotalAmountAsync(mergedIngredients);

        var normalizedOrder = order with
        {
            Ingredients = mergedIngredients,
            Recipes = []
        };

        return await _orderRepository.CreateOrderAsync(normalizedOrder, totalAmount);
    }

    /// <summary>
    /// Expands the ingredients in an order by including the ingredients from any recipes.
    /// </summary>
    /// <param name="order">The <see cref="OrderCreateDto"/> containing the ingredients and recipes to expand.</param>
    /// <returns>Returns a list of <see cref="OrderItemCreateDto"/> representing the expanded ingredients for the order.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if a recipe or ingredient is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a recipe contains no ingredients.</exception>
    private async Task<List<OrderItemCreateDto>> ExpandIngredientsAsync(OrderCreateDto order)
    {
        var expandedIngredients = new List<OrderItemCreateDto>();

        foreach (var ingredient in order.Ingredients ?? [])
        {
            ValidateQuantity(ingredient.Quantity, "Ingredient quantities must be greater than zero", order);
            expandedIngredients.Add(ingredient);
        }

        foreach (var recipeItem in order.Recipes ?? [])
        {
            ValidateQuantity(recipeItem.Quantity, "Recipe quantities must be greater than zero", order);

            var recipe = await _recipeRepository.GetRecipeByIdAsync(recipeItem.RecipeId)
                ?? throw new KeyNotFoundException($"Recipe with ID {recipeItem.RecipeId} not found");

            if (recipe.Ingredients is not { Count: > 0 })
                throw new InvalidOperationException($"Recipe with ID {recipeItem.RecipeId} contains no ingredients");

            expandedIngredients.AddRange(recipe.Ingredients.Select(ingredient => new OrderItemCreateDto(
                ingredient.Ingredient.IngredientId,
                ingredient.Amount * recipeItem.Quantity)));
        }

        return expandedIngredients;
    }

    /// <summary>
    /// Merges ingredients with the same ID by summing their quantities.
    /// </summary>
    /// <param name="ingredients">The collection of <see cref="OrderItemCreateDto"/> to merge.</param>
    /// <returns>Returns a list of <see cref="OrderItemCreateDto"/> representing the merged ingredients.</returns>
    private static List<OrderItemCreateDto> MergeIngredients(IEnumerable<OrderItemCreateDto> ingredients)
    {
        return ingredients
            .GroupBy(item => item.IngredientId)
            .Select(group => new OrderItemCreateDto(group.Key, group.Sum(item => item.Quantity)))
            .ToList();
    }

    /// <summary>
    /// Calculates the total amount of an order based on the provided ingredients.
    /// </summary>
    /// <param name="ingredients">The collection of <see cref="OrderItemCreateDto"/> representing the ingredients in the order.</param>
    /// <returns>Returns the calculated total amount of the order.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if an ingredient is not found in the repository.</exception>
    private async Task<decimal> CalculateTotalAmountAsync(IEnumerable<OrderItemCreateDto> ingredients)
    {
        decimal totalAmount = 0;

        foreach (var item in ingredients)
        {
            var ingredient = await _ingredientRepository.GetIngredientByIdAsync(item.IngredientId)
                ?? throw new KeyNotFoundException($"Ingredient with ID {item.IngredientId} not found");

            totalAmount += item.Quantity * ingredient.NetPrice;
        }

        return totalAmount;
    }

    /// <summary>
    /// Validates that the quantity is greater than zero.
    /// </summary>
    /// <param name="quantity">The quantity to validate.</param>
    /// <param name="message">The error message to use if the quantity is invalid.</param>
    /// <param name="order">The <see cref="OrderCreateDto"/> being validated, used for exception context.</param>
    /// <exception cref="ArgumentException">Thrown if the quantity is less than or equal to zero.</exception>
    private static void ValidateQuantity(decimal quantity, string message, OrderCreateDto order)
    {
        if (quantity <= 0)
            throw new ArgumentException(message, nameof(order));
    }
}