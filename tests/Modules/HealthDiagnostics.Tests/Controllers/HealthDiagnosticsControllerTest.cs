using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using HealthDiagnostics.Controllers;
using HealthDiagnostics.Models;
using HealthDiagnostics.Services;
using HealthDiagnostics.Tests.TestData;

namespace HealthDiagnostics.Tests.Controllers;

[Trait("Category", "Controller")]
[Trait("Module", "HealthDiagnostics")]
public class HealthDiagnosticsControllerTest
{
    #region GetFullStatus Tests

    [Fact]
    [Trait("Action", "GetFullStatus")]
    public async Task GetFullStatus_ShouldReturnOk_WhenHealthStatusIsHealthy()
    {
        // Arrange
        var expectedResponse = HealthDiagnosticsTestData.CreateHealthStatusResponse("Healthy");
        var serviceMock = new Mock<IHealthDiagnosticsServices>(MockBehavior.Strict);
        serviceMock
            .Setup(s => s.GetHealthReportAsync(null, false, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var controller = new HealthDiagnosticsController(serviceMock.Object);

        // Act
        var result = await controller.GetFullStatus(probeEndpoints: false, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedResponse, okResult.Value);
    }

    [Fact]
    [Trait("Action", "GetFullStatus")]
    public async Task GetFullStatus_ShouldReturnServiceUnavailable_WhenHealthStatusIsUnhealthy()
    {
        // Arrange
        var expectedResponse = HealthDiagnosticsTestData.CreateHealthStatusResponse("Unhealthy");
        var serviceMock = new Mock<IHealthDiagnosticsServices>(MockBehavior.Strict);
        serviceMock
            .Setup(s => s.GetHealthReportAsync(null, true, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var controller = new HealthDiagnosticsController(serviceMock.Object);

        // Act
        var result = await controller.GetFullStatus(probeEndpoints: true, CancellationToken.None);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusResult.StatusCode);
        Assert.Same(expectedResponse, statusResult.Value);
    }

    #endregion

    #region GetDatabaseStatus Tests

    [Fact]
    [Trait("Action", "GetDatabaseStatus")]
    public async Task GetDatabaseStatus_ShouldReturnOk_WhenPredicateMatchesHealthyDatabase()
    {
        // Arrange
        var expectedResponse = HealthDiagnosticsTestData.CreateHealthStatusResponse("Healthy");
        var serviceMock = new Mock<IHealthDiagnosticsServices>(MockBehavior.Strict);
        serviceMock
            .Setup(s => s.GetHealthReportAsync(
                It.IsNotNull<Func<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration, bool>>(),
                false,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var controller = new HealthDiagnosticsController(serviceMock.Object);

        // Act
        var result = await controller.GetDatabaseStatus(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedResponse, okResult.Value);
    }

    [Fact]
    [Trait("Action", "GetDatabaseStatus")]
    public async Task GetDatabaseStatus_ShouldReturn503_WhenDatabaseIsUnhealthy()
    {
        // Arrange
        var expectedResponse = HealthDiagnosticsTestData.CreateHealthStatusResponse("Unhealthy");
        var serviceMock = new Mock<IHealthDiagnosticsServices>(MockBehavior.Strict);
        serviceMock
            .Setup(s => s.GetHealthReportAsync(
                It.IsNotNull<Func<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration, bool>>(),
                false,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var controller = new HealthDiagnosticsController(serviceMock.Object);

        // Act
        var result = await controller.GetDatabaseStatus(CancellationToken.None);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusResult.StatusCode);
        Assert.Same(expectedResponse, statusResult.Value);
    }

    #endregion

    #region ProbeAllEndpoints Tests

    [Fact]
    [Trait("Action", "ProbeAllEndpoints")]
    public async Task ProbeAllEndpoints_ShouldReturnOkWithResults()
    {
        // Arrange
        IReadOnlyList<EndpointProbeResultDto> expectedProbes =
        [
            new("/api/store", "GET", "Healthy", 200, "10.0 ms", "OK", null)
        ];

        var serviceMock = new Mock<IHealthDiagnosticsServices>(MockBehavior.Strict);
        serviceMock
            .Setup(s => s.ProbeAllEndpointsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProbes);

        var controller = new HealthDiagnosticsController(serviceMock.Object);

        // Act
        var result = await controller.ProbeAllEndpoints(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedProbes, okResult.Value);
    }

    #endregion

    #region GetLive Tests

    [Fact]
    [Trait("Action", "GetLive")]
    public void GetLive_ShouldReturnOkWithLiveStatusAndRecentTimestamp()
    {
        // Arrange
        var serviceMock = new Mock<IHealthDiagnosticsServices>(MockBehavior.Strict);
        var controller = new HealthDiagnosticsController(serviceMock.Object);
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = controller.GetLive();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.NotNull(okResult.Value);

        var statusProperty = okResult.Value.GetType().GetProperty("status");
        var timestampProperty = okResult.Value.GetType().GetProperty("timestamp");

        Assert.NotNull(statusProperty);
        Assert.NotNull(timestampProperty);
        Assert.Equal("Live", statusProperty.GetValue(okResult.Value));

        var timestamp = Assert.IsType<DateTime>(timestampProperty.GetValue(okResult.Value));
        Assert.InRange(timestamp, beforeCall.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    #endregion
}