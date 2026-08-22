using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using ModuleCatalog.Services;
using ModuleCatalog.Tests.TestData;

using SharedKernel.Modules;

namespace ModuleCatalog.Tests.Services;

public class ModuleCatalogServiceTest
{
    private static ModuleCatalogService CreateService(
        IReadOnlyList<Endpoint>? endpoints = null,
        IEnumerable<IModule>? modules = null)
    {
        var endpointDataSourceMock = new Mock<EndpointDataSource>();
        endpointDataSourceMock
            .SetupGet(ds => ds.Endpoints)
            .Returns(endpoints ?? []);

        return new ModuleCatalogService(endpointDataSourceMock.Object, modules ?? []);
    }

    [Fact]
    public void GetRegisteredEndpoints_ShouldMapFilterAndSortRouteEndpoints()
    {
        // Arrange
        var service = CreateService(endpoints: ModuleCatalogTestData.RegisteredEndpoints);

        // Act
        var result = service.GetRegisteredEndpoints();

        // Assert
        Assert.Collection(
            result,
            endpoint =>
            {
                Assert.Equal("health", endpoint.Route);
                Assert.Equal(["*"], endpoint.HttpMethods);
                Assert.Equal("Health check", endpoint.DisplayName);
            },
            endpoint =>
            {
                Assert.Equal("orders", endpoint.Route);
                Assert.Equal(["POST"], endpoint.HttpMethods);
                Assert.Equal("Create order", endpoint.DisplayName);
            },
            endpoint =>
            {
                Assert.Equal("orders/{id}", endpoint.Route);
                Assert.Equal(["GET"], endpoint.HttpMethods);
                Assert.Equal("Get order", endpoint.DisplayName);
            });
    }

    [Fact]
    public void GetRegisteredEndpoints_ShouldFilterOutNonRouteEndpoints()
    {
        // Arrange
        var nonRouteEndpoint = new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(), "Raw Endpoint");
        var service = CreateService(endpoints: [nonRouteEndpoint]);

        // Act
        var result = service.GetRegisteredEndpoints();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetRegisteredEndpoints_ShouldDefaultToWildcardMethod_WhenHttpMethodMetadataMissing()
    {
        // Arrange
        var endpointWithoutMethod = ModuleCatalogTestData.CreateRouteEndpoint("ping", "Ping endpoint", httpMethods: null);
        var service = CreateService(endpoints: [endpointWithoutMethod]);

        // Act
        var result = service.GetRegisteredEndpoints();

        // Assert
        var singleResult = Assert.Single(result);
        Assert.Equal(["*"], singleResult.HttpMethods);
    }

    [Fact]
    public void GetRegisteredEndpoints_ShouldReturnEmptyList_WhenNoEndpointsRegistered()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = service.GetRegisteredEndpoints();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetInstalledModules_ShouldMapAndSortModulesBySlug()
    {
        // Arrange
        var service = CreateService(modules: ModuleCatalogTestData.InstalledModules);

        // Act
        var result = service.GetInstalledModules();

        // Assert
        Assert.Collection(
            result,
            module =>
            {
                Assert.Equal("module-catalog", module.Slug);
                Assert.Equal("Registered Modules", module.DisplayName);
                Assert.Equal("Displays all registered modules.", module.Description);
                Assert.Equal("SystemFeature", module.Kind);
                Assert.Equal("api/modules", module.Url);
            },
            module =>
            {
                Assert.Equal("store", module.Slug);
                Assert.Equal("Grocery Store", module.DisplayName);
                Assert.Null(module.Description);
                Assert.Equal("Standard", module.Kind);
                Assert.Equal("api/store", module.Url);
            });
    }

    [Fact]
    public void GetInstalledModules_ShouldReturnEmptyList_WhenNoModulesInstalled()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = service.GetInstalledModules();

        // Assert
        Assert.Empty(result);
    }
}