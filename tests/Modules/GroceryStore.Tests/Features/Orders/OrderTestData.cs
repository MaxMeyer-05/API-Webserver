using GroceryStore.Database.Entities;

using GroceryStore.Features.Ingredients;
using GroceryStore.Features.Orders;

namespace GroceryStore.Tests.Features.Orders;

public static class OrderTestData
{
    #region Entity Fixtures

    public static Order CreateOrder(
        int id = 0,
        Guid? customerId = null,
        decimal totalAmount = 25.80m,
        bool isCanceled = false,
        bool isCompleted = false,
        List<OrderItem>? items = null,
        Customer? customer = null) => new()
    {
        Id = id,
        CustomerId = customerId ?? Guid.NewGuid(),
        OrderDate = DateTime.UtcNow,
        TotalAmount = totalAmount,
        IsCanceled = isCanceled,
        IsCompleted = isCompleted,
        OrderItems = items ?? [],
        Customer = customer!
    };

    public static OrderItem CreateOrderItem(
        int id = 0,
        int orderId = 0,
        int ingredientId = 1,
        int quantity = 2,
        Ingredient? ingredient = null) => new()
    {
        Id = id,
        OrderId = orderId,
        IngredientId = ingredientId,
        Quantity = quantity,
        Ingredient = ingredient!
    };

    #endregion

    #region DTO Fixtures

    public static OrderDto CreateOrderDto(
        int customerOrderNumber = 1,
        Guid? customerId = null,
        decimal totalAmount = 15.50m,
        bool isCanceled = false,
        bool isCompleted = false,
        List<OrderItemDto>? items = null) => new(
            OrderId: 1,
            CustomerOrderNumber: customerOrderNumber,
            CustomerId: customerId ?? Guid.NewGuid(),
            OrderDate: DateTime.UtcNow,
            TotalAmount: totalAmount,
            IsCanceled: isCanceled,
            IsCompleted: isCompleted,
            Items: items ?? []);

    public static OrderCreateDto CreateOrderCreateDto(
        Guid? customerId = null,
        List<OrderItemCreateDto>? items = null) => new(
        CustomerId: customerId ?? Guid.NewGuid(),
        Ingredients: items ?? [new OrderItemCreateDto(1, 2)]);

    public static OrderUpdateDto CreateOrderUpdateDto(
        bool? isCanceled = null,
        bool? isCompleted = null) => new(
        IsCanceled: isCanceled,
        IsCompleted: isCompleted);

    public static OrderItemDto CreateOrderItemDto(
        int ingredientId = 1,
        string ingredientName = "Bio-Milch",
        decimal netPrice = 1.29m,
        int quantity = 2) => new(
        Ingredient: new IngredientDto(
            IngredientId: ingredientId,
            SupplierIngredientCount: 0,
            Name: ingredientName,
            Unit: "Liter",
            NetPrice: netPrice,
            Stock: 50,
            SupplierId: Guid.NewGuid(),
            SupplierName: "Biohof Nord",
            Calories: 64m,
            Carbohydrates: 4.8m,
            Protein: 3.4m),
        Quantity: quantity);

    #endregion
}