using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using HealthDiagnostics.Models;

namespace HealthDiagnostics.Services;

public class HealthDiagnosticsServices : IHealthDiagnosticsServices
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EndpointDataSource _endpointDataSource;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HealthDiagnosticsServices(
        HealthCheckService healthCheckService,
        IHttpClientFactory httpClientFactory,
        EndpointDataSource endpointDataSource,
        IHttpContextAccessor httpContextAccessor)
    {
        _healthCheckService = healthCheckService;
        _httpClientFactory = httpClientFactory;
        _endpointDataSource = endpointDataSource;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public async Task<HealthStatusResponse> GetHealthReportAsync(
        Func<HealthCheckRegistration, bool>? predicate = null,
        bool includeEndpointProbes = false,
        string? baseAddress = null,
        CancellationToken ct = default)
    {
        var report = predicate == null
            ? await _healthCheckService.CheckHealthAsync(ct)
            : await _healthCheckService.CheckHealthAsync(predicate, ct);

        var checks = report.Entries.Select(entry => new HealthEntryDto(
            Name: entry.Key,
            Status: entry.Value.Status.ToString(),
            Duration: $"{entry.Value.Duration.TotalMilliseconds:F1} ms",
            Description: entry.Value.Description,
            Error: entry.Value.Exception?.Message
        )).ToList();

        IReadOnlyList<EndpointProbeResultDto>? endpointProbes = null;
        if (includeEndpointProbes)
        {
            endpointProbes = await ProbeAllEndpointsAsync(baseAddress, ct);
        }

        return new HealthStatusResponse(
            Status: report.Status.ToString(),
            TotalDuration: $"{report.TotalDuration.TotalMilliseconds:F1} ms",
            CheckedAtUtc: DateTime.UtcNow,
            Checks: checks,
            Endpoints: endpointProbes
        );
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> ProbeEndpointAsync(string url, CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("HealthProbeClient");
            var response = await client.GetAsync(url, ct);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"Endpoint '{url}' answered with {response.StatusCode} ({(int)response.StatusCode}).")
                : HealthCheckResult.Degraded($"Endpoint '{url}' returned status {response.StatusCode} ({(int)response.StatusCode}).");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Error calling '{url}'.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EndpointProbeResultDto>> ProbeAllEndpointsAsync(
        string? baseAddress = null,
        CancellationToken ct = default)
    {
        var targetBase = baseAddress ?? ResolveCurrentBaseAddress();
        var probeRoutes = GetProbeableRoutes();
        var results = new List<EndpointProbeResultDto>();

        foreach (var route in probeRoutes)
        {
            var fullUrl = $"{targetBase.TrimEnd('/')}/{route.TrimStart('/')}";
            var sw = Stopwatch.StartNew();

            try
            {
                var checkResult = await ProbeEndpointAsync(fullUrl, ct);
                sw.Stop();

                results.Add(new EndpointProbeResultDto(
                    Route: route,
                    HttpMethod: "GET",
                    Status: checkResult.Status.ToString(),
                    StatusCode: checkResult.Status == HealthStatus.Healthy ? 200 : null,
                    Duration: $"{sw.ElapsedMilliseconds:F1} ms",
                    Description: checkResult.Description,
                    Error: checkResult.Exception?.Message
                ));
            }
            catch (Exception ex)
            {
                sw.Stop();
                results.Add(new EndpointProbeResultDto(
                    Route: route,
                    HttpMethod: "GET",
                    Status: nameof(HealthStatus.Unhealthy),
                    StatusCode: null,
                    Duration: $"{sw.ElapsedMilliseconds:F1} ms",
                    Description: "Endpoint could not be reached.",
                    Error: ex.Message
                ));
            }
        }

        return results;
    }

    /// <summary>
    /// Probes all registered endpoints and returns their health check results.
    /// </summary>
    /// <returns>The list of endpoint probe results.</returns>
    private IEnumerable<string> GetProbeableRoutes()
    {
        return _endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Where(e =>
            {
                var httpMethods = e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods;
                bool isGet = httpMethods == null || httpMethods.Contains("GET");
                var pattern = e.RoutePattern.RawText ?? string.Empty;

                bool hasParameters = pattern.Contains('{') || pattern.Contains('}');
                bool isHealthRoute = pattern.StartsWith("api/health", StringComparison.OrdinalIgnoreCase);

                return isGet && !hasParameters && !isHealthRoute && !string.IsNullOrWhiteSpace(pattern);
            })
            .Select(e => "/" + (e.RoutePattern.RawText ?? string.Empty).TrimStart('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(r => r);
    }

    /// <summary>
    /// Resolves the current base address from the HttpContext, 
    /// or returns a default if not available.
    /// </summary>
    /// <returns>The resolved base address as a string.</returns>
    private string ResolveCurrentBaseAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            return $"{context.Request.Scheme}://{context.Request.Host}";
        }

        return "http://localhost:8080"; // Fallback base address if HttpContext is not available
    }
}