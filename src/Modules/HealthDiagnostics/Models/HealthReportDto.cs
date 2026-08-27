namespace HealthDiagnostics.Models;

/// <summary>
/// Represents a single health check entry in the health report.
/// </summary>
/// <param name="Name">The name of the health check.</param>
/// <param name="Status">The status of the health check.</param>
/// <param name="Duration">The duration of the health check.</param>
/// <param name="Description">A description of the health check.</param>
/// <param name="Error">Any error message associated with the health check.</param>
public record HealthEntryDto(
    string Name,
    string Status,
    string Duration,
    string? Description,
    string? Error
);

/// <summary>
/// Represents the result of probing an endpoint for health diagnostics.
/// </summary>
/// <param name="Route">The route of the endpoint.</param>
/// <param name="HttpMethod">The HTTP method used to probe the endpoint.</param>
/// <param name="Status">The health status of the endpoint.</param>
/// <param name="StatusCode">The HTTP status code returned by the endpoint.</param>
/// <param name="Duration">The duration of the probe.</param>
/// <param name="Description">A description of the probe result.</param>
/// <param name="Error">Any error message associated with the probe.</param>
public record EndpointProbeResultDto(
    string Route,
    string HttpMethod,
    string Status,
    int? StatusCode,
    string Duration,
    string? Description,
    string? Error
);

/// <summary>
/// Represents the overall health status response,
/// including individual health checks and endpoint probe results.
/// </summary>
/// <param name="Status">The overall health status.</param>
/// <param name="TotalDuration">The total duration of all health checks.</param>
/// <param name="CheckedAtUtc">The timestamp when the health checks were performed.</param>
/// <param name="Checks">The list of individual health check entries.</param>
/// <param name="Endpoints">The list of endpoint probe results.</param>
public record HealthStatusResponse(
    string Status,
    string TotalDuration,
    DateTime CheckedAtUtc,
    IReadOnlyList<HealthEntryDto> Checks,
    IReadOnlyList<EndpointProbeResultDto>? Endpoints = null
);