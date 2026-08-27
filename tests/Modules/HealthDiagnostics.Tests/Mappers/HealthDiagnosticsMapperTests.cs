using System.Net;

using HealthDiagnostics.Mappers;
using HealthDiagnostics.Tests.TestData;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDiagnostics.Tests.Mappers;

[Trait("Category", "Mapper")]
[Trait("Module", "HealthDiagnostics")]
public class HealthDiagnosticsMapperTests
{
    private readonly HealthDiagnosticsMapper _mapper = new();

    [Fact]
    public void ToHealthStatusResponse_ShouldMapReportAndTimestamp()
    {
        // Arrange
        var report = HealthDiagnosticsTestData.CreateHealthReport(
            checkName: "database",
            description: "Database operational");
        var checkedAtUtc = new DateTime(2026, 8, 22, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var response = _mapper.ToHealthStatusResponse(report, null, checkedAtUtc);

        // Assert
        Assert.Equal("Healthy", response.Status);
        Assert.Equal("50.2 ms", response.TotalDuration);
        Assert.Equal(checkedAtUtc, response.CheckedAtUtc);
        Assert.Null(response.Endpoints);

        var check = Assert.Single(response.Checks);
        Assert.Equal("database", check.Name);
        Assert.Equal("42.5 ms", check.Duration);
        Assert.Equal("Database operational", check.Description);
    }

    [Fact]
    public void ToEndpointProbeResult_ShouldMapStatusCodeDurationAndError()
    {
        // Arrange
        var exception = new HttpRequestException("Connection refused");
        var result = HealthCheckResult.Unhealthy("Endpoint unavailable.", exception);

        // Act
        var response = _mapper.ToEndpointProbeResult(
            "/api/store",
            "GET",
            result,
            HttpStatusCode.ServiceUnavailable,
            TimeSpan.FromMilliseconds(12.34));

        // Assert
        Assert.Equal("/api/store", response.Route);
        Assert.Equal("GET", response.HttpMethod);
        Assert.Equal("Unhealthy", response.Status);
        Assert.Equal(503, response.StatusCode);
        Assert.Equal("12.3 ms", response.Duration);
        Assert.Equal("Endpoint unavailable.", response.Description);
        Assert.Equal("Connection refused", response.Error);
    }
}