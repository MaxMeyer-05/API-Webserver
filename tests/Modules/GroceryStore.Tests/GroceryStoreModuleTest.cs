using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

    #endregion
}