using Moq;
using Moq.Protected;

using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using HealthDiagnostics.Models;

namespace HealthDiagnostics.Tests.TestData;

public static class HealthDiagnosticsTestData
{
    #region Health Check & Report Fixtures

    public static HealthReport CreateHealthReport(
        HealthStatus status = HealthStatus.Healthy,
        string checkName = "test-check",
        string? description = "Check active",
        Exception? exception = null)
    {
        var entries = new Dictionary<string, HealthReportEntry>
        {
            [checkName] = new(
                status,
                description,
                TimeSpan.FromMilliseconds(42.5),
                exception,
                new Dictionary<string, object>(),
                ["system", "ready"])
        };

        return new HealthReport(entries, TimeSpan.FromMilliseconds(50.2));
    }

    public static HealthStatusResponse CreateHealthStatusResponse(
        string status = "Healthy",
        bool includeEndpoints = false)
    {
        var checks = new List<HealthEntryDto>
        {
            new("database", status, "12.3 ms", "Database reachable", null)
        };

        var endpoints = includeEndpoints
            ? new List<EndpointProbeResultDto>
            {
                new("/api/store", "GET", "Healthy", 200, "15.0 ms", "Endpoint answered with OK", null)
            }
            : null;

        return new HealthStatusResponse(
            Status: status,
            TotalDuration: "12.3 ms",
            CheckedAtUtc: DateTime.UtcNow,
            Checks: checks,
            Endpoints: endpoints);
    }

    #endregion

    #region Endpoint & Route Fixtures

    public static IReadOnlyList<Endpoint> CreateEndpoints() =>
    [
        CreateRouteEndpoint("api/store", ["GET"]),
        CreateRouteEndpoint("api/orders/{id}", ["GET"]),
        CreateRouteEndpoint("api/users", ["POST"]),
        CreateRouteEndpoint("api/health/status", ["GET"]),
        CreateRouteEndpoint("api/catalog", null), 
        new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(), "Non-route Endpoint")
    ];

    public static RouteEndpoint CreateRouteEndpoint(
        string route,
        IReadOnlyList<string>? httpMethods = null,
        string? displayName = null)
    {
        var metadata = httpMethods is null
            ? new EndpointMetadataCollection()
            : new EndpointMetadataCollection(new HttpMethodMetadata(httpMethods));

        return new RouteEndpoint(
            _ => Task.CompletedTask,
            RoutePatternFactory.Parse(route),
            0,
            metadata,
            displayName ?? route);
    }

    #endregion

    #region HTTP Client Mocking

    public static IHttpClientFactory CreateMockHttpClientFactory(
        HttpStatusCode statusCode,
        string responseContent = "OK")
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseContent)
            });

        var client = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factoryMock
            .Setup(f => f.CreateClient("HealthProbeClient"))
            .Returns(client);

        return factoryMock.Object;
    }

    #endregion
}