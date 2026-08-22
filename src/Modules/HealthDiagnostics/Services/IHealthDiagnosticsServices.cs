using HealthDiagnostics.Models;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDiagnostics.Services;

public interface IHealthDiagnosticsServices
{
    /// <summary>
    /// Returns a health report for the server and its modules, 
    /// optionally filtered by a predicate and including endpoint probes.
    /// </summary>
    /// <param name="predicate">A predicate to filter which health checks to include.</param>
    /// <param name="includeEndpointProbes">Indicates whether to include endpoint probe results.</param>
    /// <param name="baseAddress">The base address for probing endpoints.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The overall health status response.</returns>
    Task<HealthStatusResponse> GetHealthReportAsync(
        Func<HealthCheckRegistration, bool>? predicate = null, 
        bool includeEndpointProbes = false,
        string? baseAddress = null,
        CancellationToken ct = default);

    /// <summary>
    /// Probes a specific endpoint and returns its health check result.
    /// </summary>
    /// <param name="url">The URL of the endpoint to probe.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The health check result of the endpoint.</returns>
    Task<HealthCheckResult> ProbeEndpointAsync(string url, CancellationToken ct = default);

    /// <summary>
    /// Probes all registered endpoints and returns their health check results.
    /// </summary>
    /// <param name="baseAddress">The base address for probing endpoints.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The list of endpoint probe results.</returns>
    Task<IReadOnlyList<EndpointProbeResultDto>> ProbeAllEndpointsAsync(
        string? baseAddress = null, 
        CancellationToken ct = default);
}