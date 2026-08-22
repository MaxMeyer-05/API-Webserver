using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;
using GroceryStore.Tests.TestData;
using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Tests.Database;

[Trait("Category", "Database")]
[Trait("Module", "GroceryStore")]
public class GroceryStoreDbContextTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;

    public GroceryStoreDbContextTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Location & Supplier CRUD Tests

    [Fact]
    [Trait("Feature", "Supplier")]
    public async Task CanInsertLocationAndSupplierWithNavigationAsync()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("20095", "Hamburg");
        var supplier = GroceryStoreTestData.CreateSupplier("Nord Frische GmbH", "20095", "info@nordfrische.de");

        // Act
        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var retrievedSupplier = await _context.Suppliers
            .Include(s => s.ZipCodeNavigation)
            .FirstOrDefaultAsync(s => s.Id == supplier.Id);

        // Assert
        Assert.NotNull(retrievedSupplier);
        Assert.Equal("Nord Frische GmbH", retrievedSupplier.CompanyName);
        Assert.NotNull(retrievedSupplier.ZipCodeNavigation);
        Assert.Equal("Hamburg", retrievedSupplier.ZipCodeNavigation.City);
    }

    #endregion

    #region Ingredient & Allergen Relationship Tests

    [Fact]
    [Trait("Feature", "Ingredients")]
    public async Task CanAssignAllergensToIngredientAsync()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Weizenmehl");
        var allergen = new Allergen
        {
            Name = "Gluten",
            SupplierId = supplier.Id
        };

        ingredient.Allergens.Add(allergen);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        // Act
        var savedIngredient = await _context.Ingredients
            .Include(i => i.Allergens)
            .FirstOrDefaultAsync(i => i.Name == "Weizenmehl");

        // Assert
        Assert.NotNull(savedIngredient);
        var singleAllergen = Assert.Single(savedIngredient.Allergens);
        Assert.Equal("Gluten", singleAllergen.Name);
    }

    #endregion

    #region Recipe & Category Relationship Tests

    [Fact]
    [Trait("Feature", "Recipes")]
    public async Task CanCreateRecipeWithCategoryAndIngredientsAsync()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var ingredient = GroceryStoreTestData.CreateIngredient(supplier.Id, "Haferflocken");
        var category = new Category { Name = "Frühstück", SupplierId = supplier.Id };
        var recipe = GroceryStoreTestData.CreateRecipe(supplier.Id, "Porridge");

        recipe.Categories.Add(category);
        recipe.RecipeIngredients.Add(new RecipeIngredient
        {
            Ingredient = ingredient,
            Amount = 100m
        });

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        // Act
        var loadedRecipe = await _context.Recipes
            .Include(r => r.Categories)
            .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r => r.Name == "Porridge");

        // Assert
        Assert.NotNull(loadedRecipe);
        Assert.Single(loadedRecipe.Categories);
        Assert.Equal("Frühstück", loadedRecipe.Categories.First().Name);

        var recipeIngredient = Assert.Single(loadedRecipe.RecipeIngredients);
        Assert.Equal(100m, recipeIngredient.Amount);
        Assert.Equal("Haferflocken", recipeIngredient.Ingredient.Name);
    }

    #endregion

    #region Order & OrderItem Tests

    [Fact]
    [Trait("Feature", "Orders")]
    public async Task CanPlaceOrderWithMultipleItemsAsync()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var user = GroceryStoreTestData.CreateUser(zipCode: location.ZipCode);
        var ingredient1 = GroceryStoreTestData.CreateIngredient(supplier.Id, "Apfel", 0.89m);
        var ingredient2 = GroceryStoreTestData.CreateIngredient(supplier.Id, "Banane", 1.19m);

        var order = GroceryStoreTestData.CreateOrder(user.Id, totalAmount: 5.06m);
        order.OrderItems.Add(new OrderItem { Ingredient = ingredient1, Quantity = 2 });
        order.OrderItems.Add(new OrderItem { Ingredient = ingredient2, Quantity = 3 });

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Users.Add(user);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var loadedOrder = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Ingredient)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        // Assert
        Assert.NotNull(loadedOrder);
        Assert.Equal(user.Id, loadedOrder.UserId);
        Assert.Equal(2, loadedOrder.OrderItems.Count);
        Assert.Contains(loadedOrder.OrderItems, item => item.Ingredient.Name == "Apfel" && item.Quantity == 2);
        Assert.Contains(loadedOrder.OrderItems, item => item.Ingredient.Name == "Banane" && item.Quantity == 3);
    }

    [Fact]
    [Trait("Feature", "Orders")]
    public async Task UpdateOrderStatus_ShouldPersistFlagsCorrectlyAsync()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = GroceryStoreTestData.CreateUser(zipCode: location.ZipCode);
        var order = GroceryStoreTestData.CreateOrder(user.Id);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        order.IsCompleted = true;
        await _context.SaveChangesAsync();

        var updatedOrder = await _context.Orders.FindAsync(order.Id);

        // Assert
        Assert.NotNull(updatedOrder);
        Assert.True(updatedOrder.IsCompleted);
        Assert.False(updatedOrder.IsCanceled);
    }

    #endregion

    #region Entity Default Constraints Tests

    [Fact]
    [Trait("Feature", "Defaults")]
    public void Entities_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var supplier = new Supplier();
        var user = new User();
        var order = new Order();

        // Assert
        Assert.NotEqual(Guid.Empty, supplier.Id);
        Assert.Equal("supplier", supplier.Role);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("user", user.Role);
        Assert.False(order.IsCanceled);
        Assert.False(order.IsCompleted);
    }

    #endregion
}