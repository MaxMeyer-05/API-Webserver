using System.Net;

using HealthDiagnostics.Models;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDiagnostics.Mappers;

public sealed class HealthDiagnosticsMapper : IHealthDiagnosticsMapper
{
    /// <inheritdoc />
    public HealthStatusResponse ToHealthStatusResponse(
        HealthReport report,
        IReadOnlyList<EndpointProbeResultDto>? endpoints,
        DateTime checkedAtUtc)
    {
        var checks = report.Entries
            .Select(entry => new HealthEntryDto(
                Name: entry.Key,
                Status: entry.Value.Status.ToString(),
                Duration: FormatDuration(entry.Value.Duration),
                Description: entry.Value.Description,
                Error: entry.Value.Exception?.Message))
            .ToList();

        return new HealthStatusResponse(
            Status: report.Status.ToString(),
            TotalDuration: FormatDuration(report.TotalDuration),
            CheckedAtUtc: checkedAtUtc,
            Checks: checks,
            Endpoints: endpoints);
    }

    /// <inheritdoc />
    public EndpointProbeResultDto ToEndpointProbeResult(
        string route,
        string httpMethod,
        HealthCheckResult result,
        HttpStatusCode? statusCode,
        TimeSpan duration)
    {
        return new EndpointProbeResultDto(
            Route: route,
            HttpMethod: httpMethod,
            Status: result.Status.ToString(),
            StatusCode: statusCode is null ? null : (int)statusCode.Value,
            Duration: FormatDuration(duration),
            Description: result.Description,
            Error: result.Exception?.Message);
    }

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> duration into a string representation in milliseconds.
    /// </summary>
    /// <param name="duration">The duration to format.</param>
    /// <returns>A string representation of the duration in milliseconds.</returns>
    private static string FormatDuration(TimeSpan duration) =>
        $"{duration.TotalMilliseconds:F1} ms";
}