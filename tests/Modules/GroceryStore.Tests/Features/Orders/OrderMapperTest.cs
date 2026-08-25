using Moq;

using GroceryStore.Features.Ingredients;
using GroceryStore.Features.Ingredients.Interfaces;
using GroceryStore.Features.Orders;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Orders;

[Trait("Category", "Unit")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Orders")]
public class OrderMapperTest
{
    private readonly Mock<IIngredientMapper> _ingredientMapperMock;
    private readonly OrderMapper _mapper;

    public OrderMapperTest()
    {
        _ingredientMapperMock = new Mock<IIngredientMapper>(MockBehavior.Strict);
        _mapper = new OrderMapper(_ingredientMapperMock.Object);
    }

    #region ToOrderDto Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToOrderDto_ShouldMapAllProperties_WhenOrderHasUserAndItems()
    {
        // Arrange
        var user = GroceryStoreTestData.CreateUser();
        var ingredient = GroceryStoreTestData.CreateIngredient(Guid.NewGuid(), "Milch");
        var order = OrderTestData.CreateOrder(
            id: 10,
            userId: user.Id,
            totalAmount: 45.00m,
            isCanceled: false,
            isCompleted: true,
            user: user);

        var orderItem = OrderTestData.CreateOrderItem(1, order.Id, ingredient.Id, 3, ingredient);
        order.OrderItems.Add(orderItem);
        user.Orders.Add(order);

        var ingredientDto = new IngredientDto(
            IngredientId: ingredient.Id,
            SupplierIngredientCount: 0,
            Name: ingredient.Name,
            Unit: ingredient.Unit,
            NetPrice: ingredient.NetPrice,
            Stock: ingredient.Stock,
            SupplierId: ingredient.SupplierId,
            SupplierName: "Test Supplier",
            Calories: null,
            Carbohydrates: null,
            Protein: null);

        _ingredientMapperMock.Setup(m => m.ToIngredientDto(ingredient)).Returns(ingredientDto);

        // Act
        var dto = _mapper.ToOrderDto(order);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(1, dto.UserOrderNumber);
        Assert.Equal(user.Id, dto.UserId);
        Assert.Equal(45.00m, dto.TotalAmount);
        Assert.False(dto.IsCanceled);
        Assert.True(dto.IsCompleted);
        Assert.Single(dto.Items);

        var itemDto = dto.Items.First();
        Assert.Equal(3, itemDto.Quantity);
        Assert.Equal("Milch", itemDto.Ingredient.Name);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToOrderDto_ShouldReturnEmptyItems_WhenOrderItemsIsNull()
    {
        // Arrange
        var user = GroceryStoreTestData.CreateUser();
        var order = OrderTestData.CreateOrder(userId: user.Id, user: user, items: []);
        user.Orders.Add(order);

        // Act
        var dto = _mapper.ToOrderDto(order);

        // Assert
        Assert.NotNull(dto);
        Assert.Empty(dto.Items);
    }

    #endregion

    #region ToOrderEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToOrderEntity_ShouldMapCreateDtoToEntityWithOrderItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = new OrderCreateDto(
            UserId: userId,
            Ingredients:
            [
                new OrderItemCreateDto(101, 2),
                new OrderItemCreateDto(102, 5)
            ]);

        // Act
        var entity = _mapper.ToOrderEntity(createDto);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(userId, entity.UserId);
        Assert.Equal(2, entity.OrderItems.Count);
        Assert.Contains(entity.OrderItems, i => i.IngredientId == 101 && i.Quantity == 2);
        Assert.Contains(entity.OrderItems, i => i.IngredientId == 102 && i.Quantity == 5);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToOrderEntity_ShouldInitializeEmptyOrderItems_WhenIngredientsListIsNull()
    {
        // Arrange
        var createDto = new OrderCreateDto(Guid.NewGuid(), null);

        // Act
        var entity = _mapper.ToOrderEntity(createDto);

        // Assert
        Assert.NotNull(entity);
        Assert.NotNull(entity.OrderItems);
        Assert.Empty(entity.OrderItems);
    }

    #endregion

    #region ToOrderItemDto & ToOrderItemEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToOrderItemDto_ShouldMapOrderItemAndUseIngredientMapper()
    {
        // Arrange
        var ingredient = GroceryStoreTestData.CreateIngredient(Guid.NewGuid(), "Eier");
        var orderItem = OrderTestData.CreateOrderItem(1, 10, ingredient.Id, 12, ingredient);

        var ingredientDto = new IngredientDto(
            IngredientId: ingredient.Id,
            SupplierIngredientCount: 0,
            Name: ingredient.Name,
            Unit: "Stück",
            NetPrice: 0.30m,
            Stock: 100,
            SupplierId: ingredient.SupplierId,
            SupplierName: "Geflügelhof",
            Calories: null,
            Carbohydrates: null,
            Protein: null);

        _ingredientMapperMock.Setup(m => m.ToIngredientDto(ingredient)).Returns(ingredientDto);

        // Act
        var dto = _mapper.ToOrderItemDto(orderItem);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(12, dto.Quantity);
        Assert.Equal("Eier", dto.Ingredient.Name);
        _ingredientMapperMock.Verify(m => m.ToIngredientDto(ingredient), Times.Once);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToOrderItemEntity_ShouldMapCreateDtoToOrderItemEntity()
    {
        // Arrange
        var itemCreateDto = new OrderItemCreateDto(205, 4);

        // Act
        var entity = _mapper.ToOrderItemEntity(itemCreateDto);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(205, entity.IngredientId);
        Assert.Equal(4, entity.Quantity);
    }

    #endregion

    #region UpdateOrderEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateOrderEntity_ShouldUpdateStatusFlags_WhenProvided()
    {
        // Arrange
        var order = OrderTestData.CreateOrder(isCanceled: false, isCompleted: false);
        var updateDto = new OrderUpdateDto(IsCanceled: true, IsCompleted: true);

        // Act
        _mapper.UpdateOrderEntity(order, updateDto);

        // Assert
        Assert.True(order.IsCanceled);
        Assert.True(order.IsCompleted);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateOrderEntity_ShouldPreserveFlags_WhenDtoPropertiesAreNull()
    {
        // Arrange
        var order = OrderTestData.CreateOrder(isCanceled: false, isCompleted: true);
        var updateDto = new OrderUpdateDto(null, null);

        // Act
        _mapper.UpdateOrderEntity(order, updateDto);

        // Assert
        Assert.False(order.IsCanceled);
        Assert.True(order.IsCompleted);
    }

    #endregion
}