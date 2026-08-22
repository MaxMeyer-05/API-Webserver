using Moq;
using Moq.Protected;

using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using HealthDiagnostics.Services;
using HealthDiagnostics.Tests.TestData;

namespace HealthDiagnostics.Tests.Services;

[Trait("Category", "Service")]
[Trait("Module", "HealthDiagnostics")]
public class HealthDiagnosticsServiceTest
{
    #region Test Setup Helpers

    private static HealthDiagnosticsServices CreateService(
        HealthReport? report = null,
        IHttpClientFactory? httpClientFactory = null,
        IReadOnlyList<Endpoint>? endpoints = null,
        HttpContext? httpContext = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        if (report is not null)
        {
            var builder = services.AddHealthChecks();
            foreach (var (key, entry) in report.Entries)
            {
                builder.AddCheck(
                    key,
                    () => new HealthCheckResult(entry.Status, entry.Description, entry.Exception),
                    tags: entry.Tags);
            }
        }
        else
        {
            services.AddHealthChecks();
        }

        var provider = services.BuildServiceProvider();
        var healthCheckService = provider.GetRequiredService<HealthCheckService>();

        var endpointDataSourceMock = new Mock<EndpointDataSource>();
        endpointDataSourceMock
            .SetupGet(ds => ds.Endpoints)
            .Returns(endpoints ?? []);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock
            .SetupGet(a => a.HttpContext)
            .Returns(httpContext);

        var defaultClientFactory = httpClientFactory ?? HealthDiagnosticsTestData.CreateMockHttpClientFactory(HttpStatusCode.OK);

        return new HealthDiagnosticsServices(
            healthCheckService,
            defaultClientFactory,
            endpointDataSourceMock.Object,
            httpContextAccessorMock.Object);
    }

    #endregion

    #region GetHealthReportAsync Tests

    [Fact]
    [Trait("Feature", "HealthReport")]
    public async Task GetHealthReportAsync_ShouldReturnAggregatedStatusAndCheckEntries()
    {
        // Arrange
        var healthReport = HealthDiagnosticsTestData.CreateHealthReport(
            status: HealthStatus.Healthy,
            checkName: "db-check",
            description: "Database operational");
        var service = CreateService(report: healthReport);

        // Act
        var response = await service.GetHealthReportAsync();

        // Assert
        Assert.Equal("Healthy", response.Status);
        Assert.Null(response.Endpoints);
        var check = Assert.Single(response.Checks);
        Assert.Equal("db-check", check.Name);
        Assert.Equal("Healthy", check.Status);
        Assert.Equal("Database operational", check.Description);
        Assert.Null(check.Error);
    }

    [Fact]
    [Trait("Feature", "HealthReport")]
    public async Task GetHealthReportAsync_ShouldFilterChecksByPredicate()
    {
        // Arrange
        var healthReport = HealthDiagnosticsTestData.CreateHealthReport(
            status: HealthStatus.Healthy,
            checkName: "db-check");
        var service = CreateService(report: healthReport);

        // Act
        var readyResult = await service.GetHealthReportAsync(predicate: reg => reg.Tags.Contains("ready"));
        var liveResult = await service.GetHealthReportAsync(predicate: reg => reg.Tags.Contains("live"));

        // Assert
        Assert.Single(readyResult.Checks);
        Assert.Empty(liveResult.Checks);
    }

    [Fact]
    [Trait("Feature", "HealthReport")]
    public async Task GetHealthReportAsync_ShouldIncludeEndpointProbes_WhenFlagIsTrue()
    {
        // Arrange
        var service = CreateService(
            report: HealthDiagnosticsTestData.CreateHealthReport(),
            endpoints: [HealthDiagnosticsTestData.CreateRouteEndpoint("api/store", ["GET"])]);

        // Act
        var response = await service.GetHealthReportAsync(includeEndpointProbes: true);

        // Assert
        Assert.NotNull(response.Endpoints);
        var probe = Assert.Single(response.Endpoints);
        Assert.Equal("/api/store", probe.Route);
        Assert.Equal("GET", probe.HttpMethod);
        Assert.Equal("Healthy", probe.Status);
        Assert.Equal(200, probe.StatusCode);
    }

    #endregion

    #region ProbeEndpointAsync Tests

    [Fact]
    [Trait("Feature", "EndpointProbing")]
    public async Task ProbeEndpointAsync_ShouldReturnHealthy_WhenStatusCodeIsSuccess()
    {
        // Arrange
        var clientFactory = HealthDiagnosticsTestData.CreateMockHttpClientFactory(HttpStatusCode.OK);
        var service = CreateService(httpClientFactory: clientFactory);

        // Act
        var result = await service.ProbeEndpointAsync("http://localhost:8080/api/store");

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Contains("answered with OK (200)", result.Description);
    }

    [Fact]
    [Trait("Feature", "EndpointProbing")]
    public async Task ProbeEndpointAsync_ShouldReturnDegraded_WhenStatusCodeIsNotSuccess()
    {
        // Arrange
        var clientFactory = HealthDiagnosticsTestData.CreateMockHttpClientFactory(HttpStatusCode.InternalServerError);
        var service = CreateService(httpClientFactory: clientFactory);

        // Act
        var result = await service.ProbeEndpointAsync("http://localhost:8080/api/store");

        // Assert
        Assert.Equal(HealthStatus.Degraded, result.Status);
        Assert.Contains("returned status InternalServerError (500)", result.Description);
    }

    [Fact]
    [Trait("Feature", "EndpointProbing")]
    public async Task ProbeEndpointAsync_ShouldReturnUnhealthy_WhenHttpClientThrowsException()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var clientFactoryMock = new Mock<IHttpClientFactory>();
        clientFactoryMock
            .Setup(f => f.CreateClient("HealthProbeClient"))
            .Returns(new HttpClient(handlerMock.Object));

        var service = CreateService(httpClientFactory: clientFactoryMock.Object);

        // Act
        var result = await service.ProbeEndpointAsync("http://localhost:8080/api/broken");

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Equal("Error calling 'http://localhost:8080/api/broken'.", result.Description);
        Assert.IsType<HttpRequestException>(result.Exception);
    }

    #endregion

    #region ProbeAllEndpointsAsync Tests

    [Fact]
    [Trait("Feature", "EndpointProbing")]
    public async Task ProbeAllEndpointsAsync_ShouldFilterNonGetParametrizedAndHealthRoutes()
    {
        // Arrange
        var endpoints = HealthDiagnosticsTestData.CreateEndpoints();
        var service = CreateService(endpoints: endpoints);

        // Act
        var results = await service.ProbeAllEndpointsAsync();

        // Assert
        Assert.Collection(
            results,
            probe => Assert.Equal("/api/catalog", probe.Route),
            probe => Assert.Equal("/api/store", probe.Route));
    }

    [Fact]
    [Trait("Feature", "EndpointProbing")]
    public async Task ProbeAllEndpointsAsync_ShouldUseBaseAddressFromHttpContext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("myserver.internal:9000");

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.AbsoluteUri.StartsWith("https://myserver.internal:9000/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var clientFactoryMock = new Mock<IHttpClientFactory>();
        clientFactoryMock
            .Setup(f => f.CreateClient("HealthProbeClient"))
            .Returns(new HttpClient(handlerMock.Object));

        var service = CreateService(
            httpClientFactory: clientFactoryMock.Object,
            endpoints: [HealthDiagnosticsTestData.CreateRouteEndpoint("api/store", ["GET"])],
            httpContext: context);

        // Act
        var results = await service.ProbeAllEndpointsAsync();

        // Assert
        var probe = Assert.Single(results);
        Assert.Equal("/api/store", probe.Route);
        Assert.Equal("Healthy", probe.Status);
    }

    #endregion
}