using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using HealthDiagnostics.Models;
using HealthDiagnostics.Services;

namespace HealthDiagnostics.Controllers;

/// <summary>
/// Controller for health diagnostics endpoints, 
/// providing information about the health status of the server and its modules.
/// </summary>
[ApiController]
[Route("api/health")]
[Tags("HealthDiagnostics")]
[Produces("application/json")]
public class HealthDiagnosticsController : ControllerBase
{
    private readonly IHealthDiagnosticsServices _diagnosticsService;
    private readonly ILogger<HealthDiagnosticsController> _logger;

    public HealthDiagnosticsController(
        IHealthDiagnosticsServices diagnosticsService, 
        ILogger<HealthDiagnosticsController> logger)
    {
        _diagnosticsService = diagnosticsService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the overall health status of the server and its modules,
    /// optionally probing endpoints for their health.
    /// </summary>
    /// <param name="probeEndpoints">Indicates whether to probe endpoints for their health.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The overall health status of the server and its modules.</returns>
    /// <response code="200">The server and its modules are healthy.</response>
    /// </response code="503">The server or one of its modules is unhealthy.</response>
    [HttpGet("status")]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetFullStatus([FromQuery] bool probeEndpoints, CancellationToken ct)
    {
        _logger.LogDebug("Received request to get full health status.");

        var report = await _diagnosticsService.GetHealthReportAsync(
            predicate: null, 
            includeEndpointProbes: probeEndpoints, 
            ct: ct);

        _logger.LogDebug("Health status report generated.");

        return report.Status == nameof(HealthStatus.Unhealthy)
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, report)
            : Ok(report);
    }

    /// <summary>
    /// Probes all registered endpoints and returns their health status.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The health status of all registered endpoints.</returns>
    /// <response code="200">The health status of all registered endpoints.</response>
    [HttpGet("endpoints")]
    [ProducesResponseType(typeof(IEnumerable<EndpointProbeResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProbeAllEndpoints(CancellationToken ct)
    {
        _logger.LogDebug("Received request to probe all endpoints.");

        var results = await _diagnosticsService.ProbeAllEndpointsAsync(ct: ct);

        _logger.LogDebug("Endpoint probe results generated.");

        return Ok(results);
    }

    /// <summary>
    /// Returns a simple "live" status indicating that the server is running.
    /// This endpoint is typically used for liveness probes in containerized environments.
    /// </summary>
    /// <returns>An object containing the live status and the current timestamp.</returns>
    /// <response code="200">The server is live and running.</response>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetLive() => Ok(new { status = "Live", timestamp = DateTime.UtcNow });

    [HttpGet("database")]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDatabaseStatus(CancellationToken ct)
    {
        _logger.LogDebug("Received request to get database health status.");

        var report = await _diagnosticsService.GetHealthReportAsync(
            predicate: check => check.Tags.Contains("ready") || check.Tags.Contains("db"),
            includeEndpointProbes: false,
            ct: ct);

        _logger.LogDebug("Database health status report generated.");

        return report.Status == nameof(HealthStatus.Unhealthy)
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, report)
            : Ok(report);
    }
}