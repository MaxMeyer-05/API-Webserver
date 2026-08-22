using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using HealthDiagnostics.Controllers;
using HealthDiagnostics.Mappers;
using HealthDiagnostics.Services;

using SharedKernel.Modules;

namespace HealthDiagnostics.Tests;

[Trait("Category", "Module")]
[Trait("Module", "HealthDiagnostics")]
public class HealthDiagnosticsModuleTest
{
    #region Metadata Tests

    [Fact]
    [Trait("Feature", "Metadata")]
    public void Properties_ShouldReturnExpectedModuleMetadata()
    {
        // Arrange
        var module = new HealthDiagnosticsModule();

        // Assert
        Assert.IsAssignableFrom<IModule>(module);
        Assert.Equal("health-diagnostics", module.Slug);
        Assert.Equal("Health Diagnostics", module.DisplayName);
        Assert.Equal("Provides health diagnostics for the server and its modules.", module.Description);
        Assert.Equal(ModuleKind.SystemFeature, module.Kind);
        Assert.Equal("modules/health-diagnostics", module.StaticFileUrlPrefix);
    }

    #endregion

    #region Service Registration Tests

    [Fact]
    [Trait("Feature", "ServiceRegistration")]
    public void ConfigureServices_ShouldRegisterRequiredDependencies()
    {
        // Arrange
        var module = new HealthDiagnosticsModule();
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        module.ConfigureServices(services, configuration);

        // Assert
        var serviceDescriptor = Assert.Single(
            services,
            d => d.ServiceType == typeof(IHealthDiagnosticsServices));
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
        Assert.Equal(typeof(HealthDiagnosticsServices), serviceDescriptor.ImplementationType);

        var mapperDescriptor = Assert.Single(
            services,
            d => d.ServiceType == typeof(IHealthDiagnosticsMapper));
        Assert.Equal(ServiceLifetime.Singleton, mapperDescriptor.Lifetime);
        Assert.Equal(typeof(HealthDiagnosticsMapper), mapperDescriptor.ImplementationType);

        Assert.Contains(services, d => d.ServiceType == typeof(IHttpContextAccessor));
        Assert.Contains(services, d => d.ServiceType == typeof(IHttpClientFactory));
    }

    [Fact]
    [Trait("Feature", "ServiceRegistration")]
    public void ConfigureServices_ShouldRegisterControllerAssembly()
    {
        // Arrange
        var module = new HealthDiagnosticsModule();
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        module.ConfigureServices(services, configuration);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var partManager = serviceProvider.GetRequiredService<ApplicationPartManager>();
        Assert.Contains(
            partManager.ApplicationParts.OfType<AssemblyPart>(),
            part => part.Assembly == typeof(HealthDiagnosticsController).Assembly);
    }

    #endregion

    #region Health Check Registration Tests

    [Fact]
    [Trait("Feature", "HealthChecks")]
    public async Task RegisterHealthChecks_ShouldRegisterHealthyDiagnosticsCheck()
    {
        // Arrange
        var module = new HealthDiagnosticsModule();
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
        Assert.Equal("health-diagnostics-self", entry.Key);
        Assert.Equal(HealthStatus.Healthy, entry.Value.Status);
        Assert.Equal("Diagnostics module active.", entry.Value.Description);
        Assert.Contains("system", entry.Value.Tags);
        Assert.Contains($"module:{module.Slug}", entry.Value.Tags);
    }

    [Fact]
    [Trait("Feature", "HealthChecks")]
    public async Task RegisterHealthChecks_ShouldFilterCorrectlyByTags()
    {
        // Arrange
        var module = new HealthDiagnosticsModule();
        var services = new ServiceCollection();
        services.AddLogging();
        var healthChecksBuilder = services.AddHealthChecks();
        module.RegisterHealthChecks(healthChecksBuilder);

        using var serviceProvider = services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();

        // Act
        var systemReport = await healthCheckService.CheckHealthAsync(r => r.Tags.Contains("system"));
        var databaseReport = await healthCheckService.CheckHealthAsync(r => r.Tags.Contains("db"));

        // Assert
        Assert.Single(systemReport.Entries);
        Assert.Empty(databaseReport.Entries);
    }

    #endregion
}