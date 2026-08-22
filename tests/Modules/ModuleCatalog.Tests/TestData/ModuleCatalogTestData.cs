using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

using ModuleCatalog.Contracts;
using SharedKernel.Modules;

namespace ModuleCatalog.Tests.TestData;

public static class ModuleCatalogTestData
{
    public static IReadOnlyList<EndpointDto> RegisteredEndpointDtos =>
    [
        new("health", ["*"], "Health check"),
        new("orders", ["POST"], "Create order"),
        new("orders/{id}", ["GET"], "Get order")
    ];

    public static IReadOnlyList<ModuleDto> InstalledModuleDtos =>
    [
        new(
            "module-catalog",
            "Registered Modules",
            "Displays all registered modules.",
            "SystemFeature",
            "api/modules"),
        new("store", "Grocery Store", null, "Standard", "api/store")
    ];

    public static IReadOnlyList<Endpoint> RegisteredEndpoints =>
    [
        CreateRouteEndpoint("orders/{id}", "Get order", ["GET"]),
        new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(), "Non-route endpoint"),
        CreateRouteEndpoint("health", "Health check"),
        CreateRouteEndpoint("orders", "Create order", ["POST"])
    ];

    public static IReadOnlyList<IModule> InstalledModules =>
    [
        CreateModule("store", "Grocery Store", null, ModuleKind.Standard, "api/store"),
        CreateModule(
            "module-catalog",
            "Registered Modules",
            "Displays all registered modules.",
            ModuleKind.SystemFeature,
            "api/modules")
    ];

    public static RouteEndpoint CreateRouteEndpoint(
        string route,
        string displayName,
        IReadOnlyList<string>? httpMethods = null)
    {
        var metadata = httpMethods is null
            ? new EndpointMetadataCollection()
            : new EndpointMetadataCollection(new HttpMethodMetadata(httpMethods));

        return new RouteEndpoint(
            _ => Task.CompletedTask,
            RoutePatternFactory.Parse(route),
            0,
            metadata,
            displayName);
    }

    public static IModule CreateModule(
        string slug,
        string displayName,
        string? description,
        ModuleKind kind,
        string staticFileUrlPrefix)
    {
        var moduleMock = new Mock<IModule>();
        moduleMock.SetupGet(m => m.Slug).Returns(slug);
        moduleMock.SetupGet(m => m.DisplayName).Returns(displayName);
        moduleMock.SetupGet(m => m.Description).Returns(description);
        moduleMock.SetupGet(m => m.Kind).Returns(kind);
        moduleMock.SetupGet(m => m.StaticFileUrlPrefix).Returns(staticFileUrlPrefix);
        return moduleMock.Object;
    }
}