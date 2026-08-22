using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using HealthDiagnostics;
using HealthDiagnostics.Controllers;

using GroceryStore;
using GroceryStore.Database.DbContexts;

using ModuleCatalog;
using ModuleCatalog.Controllers;

using Server.Database.DbContexts;
using Server.Extensions;
using Server.Tests.TestData;

using SystemSettings;
using SystemSettings.Controllers;

using SharedKernel.Modules;

namespace Server.Tests.Extensions;

[Trait("Category", "ServiceRegistration")]
[Trait("Module", "Server")]
public class ServiceRegistrationTest
{
    #region Test Setup Helpers

    private static WebApplicationBuilder CreateBuilder(IConfiguration? configuration = null)
    {
        var config = configuration ?? ServerTestData.CreateConfiguration();
        var builder = WebApplication.CreateSlimBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddConfiguration(config);
        return builder;
    }

    #endregion

    #region Module Registration Tests

    [Fact]
    [Trait("Feature", "Modules")]
    public void AddServerServices_ShouldRegisterAndReturnAllCoreModules()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var returnedModules = builder.AddServerServices();

        // Assert
        Assert.NotNull(returnedModules);
        Assert.Equal(4, returnedModules.Count);
        Assert.Contains(returnedModules, m => m is ModuleCatalogModule);
        Assert.Contains(returnedModules, m => m is GroceryStoreModule);
        Assert.Contains(returnedModules, m => m is HealthDiagnosticsModule);
        Assert.Contains(returnedModules, m => m is SystemSettingsModule);
    }

    [Fact]
    [Trait("Feature", "Modules")]
    public void AddServerServices_ShouldRegisterModuleCollectionsInDependencyInjection()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.AddServerServices();

        using var serviceProvider = builder.Services.BuildServiceProvider();

        // Act
        var enumerableModules = serviceProvider.GetService<IEnumerable<IModule>>();
        var readOnlyListModules = serviceProvider.GetService<IReadOnlyList<IModule>>();

        // Assert
        Assert.NotNull(enumerableModules);
        Assert.NotNull(readOnlyListModules);
        Assert.Equal(4, enumerableModules.Count());
        Assert.Equal(4, readOnlyListModules.Count);
    }

    #endregion

    #region Database Registration Tests

    [Fact]
    [Trait("Feature", "Database")]
    public void AddServerServices_ShouldRegisterDbContexts_WhenConnectionStringsAreProvided()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.AddServerServices();

        using var serviceProvider = builder.Services.BuildServiceProvider();

        // Act
        var groceryDbContext = serviceProvider.GetService<GroceryStoreDbContext>();
        var serverDbContext = serviceProvider.GetService<ServerDbContext>();

        // Assert
        Assert.NotNull(groceryDbContext);
        Assert.NotNull(serverDbContext);
    }

    [Fact]
    [Trait("Feature", "Database")]
    public void AddServerServices_ShouldThrowInvalidOperationException_WhenGroceryStoreConnectionStringIsMissing()
    {
        // Arrange
        var emptyConfig = ServerTestData.CreateConfiguration(groceryStoreDb: null, serverDb: "Data Source=:memory:");
        var builder = CreateBuilder(emptyConfig);

        // Act
        builder.AddServerServices();
        using var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<GroceryStoreDbContext>());
        Assert.Contains("GroceryStore", ex.Message);
    }

    [Fact]
    [Trait("Feature", "Database")]
    public void AddServerServices_ShouldThrowInvalidOperationException_WhenServerConnectionStringIsMissing()
    {
        // Arrange
        var emptyConfig = ServerTestData.CreateConfiguration(groceryStoreDb: "Data Source=:memory:", serverDb: null);
        var builder = CreateBuilder(emptyConfig);

        // Act
        builder.AddServerServices();
        using var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<ServerDbContext>());
        Assert.Contains("Server", ex.Message);
    }

    #endregion

    #region Health Check Registration Tests

    [Fact]
    [Trait("Feature", "HealthChecks")]
    public async Task AddServerServices_ShouldRegisterDatabaseHealthChecksWithExpectedTags()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.AddServerServices();

        using var serviceProvider = builder.Services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();

        // Act
        var readyReport = await healthCheckService.CheckHealthAsync(registration => registration.Tags.Contains("db"));

        // Assert
        Assert.Contains("grocerystore-db", readyReport.Entries.Keys);
        Assert.Contains("server-db", readyReport.Entries.Keys);
        Assert.Contains("ready", readyReport.Entries["grocerystore-db"].Tags);
        Assert.Contains("ready", readyReport.Entries["server-db"].Tags);
    }

    [Fact]
    [Trait("Feature", "HealthChecks")]
    public async Task AddServerServices_ShouldRegisterModuleSelfHealthChecks()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.AddServerServices();

        using var serviceProvider = builder.Services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();

        // Act
        var report = await healthCheckService.CheckHealthAsync();

        // Assert
        Assert.Contains("module-catalog-self", report.Entries.Keys);
        Assert.Contains("health-diagnostics-self", report.Entries.Keys);
    }

    #endregion

    #region API & Controller Configuration Tests

    [Fact]
    [Trait("Feature", "ApiConfiguration")]
    public void AddServerServices_ShouldRegisterApplicationPartsForModuleControllers()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.AddServerServices();

        using var serviceProvider = builder.Services.BuildServiceProvider();
        var partManager = serviceProvider.GetRequiredService<ApplicationPartManager>();

        var registeredAssemblies = partManager.ApplicationParts
            .OfType<AssemblyPart>()
            .Select(p => p.Assembly)
            .ToHashSet();

        // Assert
        Assert.Contains(typeof(HealthDiagnosticsController).Assembly, registeredAssemblies);
        Assert.Contains(typeof(ModuleCatalogController).Assembly, registeredAssemblies);
        Assert.Contains(typeof(SystemSettingsController).Assembly, registeredAssemblies);
    }

    [Fact]
    [Trait("Feature", "ApiConfiguration")]
    public void AddServerServices_ShouldConfigureReturnHttpNotAcceptableOption()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.AddServerServices();

        using var serviceProvider = builder.Services.BuildServiceProvider();
        var mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcOptions>>().Value;

        // Assert
        Assert.True(mvcOptions.ReturnHttpNotAcceptable);
    }

    #endregion
}