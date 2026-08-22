using System.Net;

using HealthDiagnostics.Models;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDiagnostics.Mappers;

public interface IHealthDiagnosticsMapper
{
    /// <summary>
    /// Maps a <see cref="HealthReport"/> to a <see cref="HealthStatusResponse"/> DTO.
    /// </summary>
    /// <param name="report">The health report to map.</param>
    /// <param name="endpoints">Optional list of endpoint probe results.</param>
    /// <param name="checkedAtUtc">The UTC timestamp when the health check was performed.</param>
    /// <returns>A <see cref="HealthStatusResponse"/> DTO representing the health status.</returns>
    HealthStatusResponse ToHealthStatusResponse(
        HealthReport report,
        IReadOnlyList<EndpointProbeResultDto>? endpoints,
        DateTime checkedAtUtc);

    /// <summary>
    /// Maps a health check result to an <see cref="EndpointProbeResultDto"/> DTO.
    /// </summary>
    /// <param name="route">The route of the endpoint.</param>
    /// <param name="httpMethod">The HTTP method used for the probe.</param>
    /// <param name="result">The health check result.</param>
    /// <param name="statusCode">The HTTP status code returned by the endpoint.</param>
    /// <param name="duration">The duration of the probe.</param>
    /// <returns>An <see cref="EndpointProbeResultDto"/> DTO representing the probe result.</returns>
    EndpointProbeResultDto ToEndpointProbeResult(
        string route,
        string httpMethod,
        HealthCheckResult result,
        HttpStatusCode? statusCode,
        TimeSpan duration);
}