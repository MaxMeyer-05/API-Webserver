using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using GroceryStore.Features.Allergens;
using GroceryStore.Features.Allergens.Interfaces;

using GroceryStore.Features.Categories;
using GroceryStore.Features.Categories.Interfaces;

using GroceryStore.Features.Ingredients;
using GroceryStore.Features.Ingredients.Interfaces;

using GroceryStore.Features.Locations;
using GroceryStore.Features.Locations.Interfaces;

using GroceryStore.Features.Orders;
using GroceryStore.Features.Orders.Interfaces;

using GroceryStore.Features.Recipes;
using GroceryStore.Features.Recipes.Interfaces;

using GroceryStore.Features.Suppliers;
using GroceryStore.Features.Suppliers.Interfaces;

using GroceryStore.Features.Users;
using GroceryStore.Features.Users.Interfaces;

using SharedKernel.Modules;

namespace GroceryStore.Tests;

[Trait("Category", "Module")]
[Trait("Module", "GroceryStore")]
public class GroceryStoreModuleTest
{
    #region Metadata Tests

    [Fact]
    [Trait("Feature", "Metadata")]
    public void Properties_ShouldReturnExpectedModuleMetadata()
    {
        // Arrange
        var module = new GroceryStoreModule();

        // Assert
        Assert.IsAssignableFrom<IModule>(module);
        Assert.Equal("grocery-store", module.Slug);
        Assert.Equal("Grocery Store", module.DisplayName);
        Assert.Equal("Manages grocery store operations.", module.Description);
        Assert.Equal(ModuleKind.Standard, module.Kind);
        Assert.Equal("modules/grocery-store", module.StaticFileUrlPrefix);
    }

    #endregion

    #region Service Configuration Tests

    [Fact]
    [Trait("Feature", "ServiceRegistration")]
    public void ConfigureServices_ShouldExecuteWithoutThrowingExceptions()
    {
        // Arrange
        var module = new GroceryStoreModule();
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        var exception = Record.Exception(() => module.ConfigureServices(services, configuration));
        Assert.Null(exception);
    }

    [Theory]
    [Trait("Feature", "ServiceRegistration")]
    [MemberData(nameof(ServiceRegistrations))]
    public void ConfigureServices_ShouldRegisterService(
        Type serviceType,
        Type implementationType,
        ServiceLifetime lifetime)
    {
        // Arrange
        var module = new GroceryStoreModule();
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        module.ConfigureServices(services, configuration);

        // Assert
        var descriptor = Assert.Single(services, service => service.ServiceType == serviceType);
        Assert.Equal(implementationType, descriptor.ImplementationType);
        Assert.Equal(lifetime, descriptor.Lifetime);
    }

    public static TheoryData<Type, Type, ServiceLifetime> ServiceRegistrations => new()
    {
        { typeof(IAllergenMapper), typeof(AllergenMapper), ServiceLifetime.Singleton },
        { typeof(ICategoryMapper), typeof(CategoryMapper), ServiceLifetime.Singleton },
        { typeof(IIngredientMapper), typeof(IngredientMapper), ServiceLifetime.Singleton },
        { typeof(ILocationMapper), typeof(LocationMapper), ServiceLifetime.Singleton },
        { typeof(IOrderMapper), typeof(OrderMapper), ServiceLifetime.Singleton },
        { typeof(IRecipeMapper), typeof(RecipeMapper), ServiceLifetime.Singleton },
        { typeof(ISupplierMapper), typeof(SupplierMapper), ServiceLifetime.Singleton },
        { typeof(IUserMapper), typeof(UserMapper), ServiceLifetime.Singleton },
        { typeof(IAllergenRepository), typeof(AllergenRepository), ServiceLifetime.Scoped },
        { typeof(ICategoryRepository), typeof(CategoryRepository), ServiceLifetime.Scoped },
        { typeof(IIngredientRepository), typeof(IngredientRepository), ServiceLifetime.Scoped },
        { typeof(ILocationRepository), typeof(LocationRepository), ServiceLifetime.Scoped },
        { typeof(IOrderRepository), typeof(OrderRepository), ServiceLifetime.Scoped },
        { typeof(IRecipeRepository), typeof(RecipeRepository), ServiceLifetime.Scoped },
        { typeof(ISupplierRepository), typeof(SupplierRepository), ServiceLifetime.Scoped },
        { typeof(IUserRepository), typeof(UserRepository), ServiceLifetime.Scoped },
        { typeof(AllergenService), typeof(AllergenService), ServiceLifetime.Scoped },
        { typeof(CategoryService), typeof(CategoryService), ServiceLifetime.Scoped },
        { typeof(IngredientService), typeof(IngredientService), ServiceLifetime.Scoped },
        { typeof(LocationService), typeof(LocationService), ServiceLifetime.Scoped },
        { typeof(OrderService), typeof(OrderService), ServiceLifetime.Scoped },
        { typeof(RecipeService), typeof(RecipeService), ServiceLifetime.Scoped },
        { typeof(SupplierService), typeof(SupplierService), ServiceLifetime.Scoped },
        { typeof(UserService), typeof(UserService), ServiceLifetime.Scoped }
    };

    #endregion
}