using Microsoft.AspNetCore.Mvc.ApplicationParts;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using ModuleCatalog.Controllers;
using ModuleCatalog.Services;

using SharedKernel.Modules;

namespace ModuleCatalog.Tests;

[Trait("Category", "Module")]
[Trait("Module", "ModuleCatalog")]
public class ModuleCatalogModuleTest
{
    #region Metadata Tests

    [Fact]
    [Trait("Feature", "Metadata")]
    public void Properties_ShouldReturnExpectedModuleMetadata()
    {
        // Arrange
        var module = new ModuleCatalogModule();

        // Assert
        Assert.IsAssignableFrom<IModule>(module);
        Assert.Equal("module-catalog", module.Slug);
        Assert.Equal("Registered Modules", module.DisplayName);
        Assert.Equal("Displays all registered modules.", module.Description);
        Assert.Equal(ModuleKind.SystemFeature, module.Kind);
        Assert.Equal("api/modules", module.StaticFileUrlPrefix);
    }

    #endregion

    #region Service Registration Tests

    [Fact]
    [Trait("Feature", "ServiceRegistration")]
    public void ConfigureServices_ShouldRegisterModuleCatalogServiceAsSingleton()
    {
        // Arrange
        var module = new ModuleCatalogModule();
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        module.ConfigureServices(services, configuration);

        // Assert
        var descriptor = Assert.Single(
            services,
            d => d.ServiceType == typeof(IModuleCatalogService));

        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Equal(typeof(ModuleCatalogService), descriptor.ImplementationType);
    }

    [Fact]
    [Trait("Feature", "ServiceRegistration")]
    public void ConfigureServices_ShouldAddControllerAssemblyToApplicationParts()
    {
        // Arrange
        var module = new ModuleCatalogModule();
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        module.ConfigureServices(services, configuration);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var partManager = serviceProvider.GetRequiredService<ApplicationPartManager>();

        var controllerAssembly = typeof(ModuleCatalogController).Assembly;
        Assert.Contains(
            partManager.ApplicationParts.OfType<AssemblyPart>(),
            part => part.Assembly == controllerAssembly);
    }

            #endregion

            #region Health Check Registration Tests

    [Fact]
            [Trait("Feature", "HealthChecks")]
    public async Task RegisterHealthChecks_ShouldRegisterHealthyModuleCheck()
    {
        // Arrange
        var module = new ModuleCatalogModule();
        var services = new ServiceCollection();
        services.AddLogging();
        var healthChecksBuilder = services.AddHealthChecks();

        // Act
        module.RegisterHealthChecks(healthChecksBuilder);

        using var serviceProvider = services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();
        var report = await healthCheckService.CheckHealthAsync();

        // Assert
        var entry = Assert.Single(report.Entries);
        Assert.Equal("module-catalog-self", entry.Key);
        Assert.Equal(HealthStatus.Healthy, entry.Value.Status);
        Assert.Equal("Module catalog module active.", entry.Value.Description);
        Assert.Contains("system", entry.Value.Tags);
        Assert.Contains($"module:{module.Slug}", entry.Value.Tags);
    }

    [Fact]
    [Trait("Feature", "HealthChecks")]
    public async Task RegisterHealthChecks_ShouldFilterCorrectlyByTags()
    {
        // Arrange
        var module = new ModuleCatalogModule();
        var services = new ServiceCollection();
        services.AddLogging();
        var healthChecksBuilder = services.AddHealthChecks();
        module.RegisterHealthChecks(healthChecksBuilder);

        using var serviceProvider = services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();

        // Act
        var systemReport = await healthCheckService.CheckHealthAsync(registration => registration.Tags.Contains("system"));
        var unrelatedReport = await healthCheckService.CheckHealthAsync(registration => registration.Tags.Contains("database"));

        // Assert
        Assert.Single(systemReport.Entries);
        Assert.Empty(unrelatedReport.Entries);
    }

    #endregion
}