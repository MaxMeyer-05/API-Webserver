using System.Net;
using System.Diagnostics;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using HealthDiagnostics.Mappers;
using HealthDiagnostics.Models;

namespace HealthDiagnostics.Services;

public class HealthDiagnosticsServices : IHealthDiagnosticsServices
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EndpointDataSource _endpointDataSource;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHealthDiagnosticsMapper _mapper;
    private readonly ILogger<HealthDiagnosticsServices> _logger;

    public HealthDiagnosticsServices(
        HealthCheckService healthCheckService,
        IHttpClientFactory httpClientFactory,
        EndpointDataSource endpointDataSource,
        IHttpContextAccessor httpContextAccessor,
        IHealthDiagnosticsMapper mapper,
        ILogger<HealthDiagnosticsServices> logger)
    {
        _healthCheckService = healthCheckService;
        _httpClientFactory = httpClientFactory;
        _endpointDataSource = endpointDataSource;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _logger = logger;
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

        IReadOnlyList<EndpointProbeResultDto>? endpointProbes = null;
        if (includeEndpointProbes)
        {
            endpointProbes = await ProbeAllEndpointsAsync(baseAddress, ct);
        }

        return _mapper.ToHealthStatusResponse(report, endpointProbes, DateTime.UtcNow);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> ProbeEndpointAsync(string url, CancellationToken ct = default)
    {
        var outcome = await ProbeEndpointCoreAsync(url, ct);
        return outcome.Result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EndpointProbeResultDto>> ProbeAllEndpointsAsync(
        string? baseAddress = null,
        CancellationToken ct = default)
    {
        var targetBase = baseAddress ?? ResolveCurrentBaseAddress();
        var probeTasks = GetProbeableRoutes()
            .Select(route => ProbeRouteAsync(route, targetBase, ct));

        return await Task.WhenAll(probeTasks);
    }

    /// <summary>
    /// Probes a specific route and returns its health check result.
    /// </summary>
    /// <param name="route">The route to probe.</param>
    /// <param name="baseAddress">The base address for probing the route.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The endpoint probe result for the specified route.</returns>
    private async Task<EndpointProbeResultDto> ProbeRouteAsync(
        string route,
        string baseAddress,
        CancellationToken ct)
    {
        var fullUrl = $"{baseAddress.TrimEnd('/')}/{route.TrimStart('/')}";
        var stopwatch = Stopwatch.StartNew();
        var outcome = await ProbeEndpointCoreAsync(fullUrl, ct);
        stopwatch.Stop();

        return _mapper.ToEndpointProbeResult(
            route,
            "GET",
            outcome.Result,
            outcome.StatusCode,
            stopwatch.Elapsed);
    }

    /// <summary>
    /// Probes a specific endpoint and returns its health check result along with the HTTP status code.
    /// </summary>
    /// <param name="url">The URL of the endpoint to probe.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The endpoint probe outcome containing the health check result and HTTP status code.</returns>
    private async Task<EndpointProbeOutcome> ProbeEndpointCoreAsync(
        string url,
        CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("HealthProbeClient");
            using var response = await client.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

            var result = response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"Endpoint '{url}' answered with {response.StatusCode} ({(int)response.StatusCode}).")
                : HealthCheckResult.Degraded($"Endpoint '{url}' returned status {response.StatusCode} ({(int)response.StatusCode}).");

            return new EndpointProbeOutcome(result, response.StatusCode);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning("Health probe for '{Url}' was canceled.", url);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while probing endpoint '{Url}'.", url);
            return new EndpointProbeOutcome(
                HealthCheckResult.Unhealthy($"Error calling '{url}'.", ex),
                null);
        }
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
                bool isGet = httpMethods == null || httpMethods.Contains("GET", StringComparer.OrdinalIgnoreCase);
                var pattern = e.RoutePattern.RawText ?? string.Empty;

                bool hasParameters = pattern.Contains('{') || pattern.Contains('}');
                bool isHealthRoute = pattern
                    .TrimStart('/')
                    .StartsWith("api/health", StringComparison.OrdinalIgnoreCase);

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

    /// <summary>
    /// Represents the outcome of probing an endpoint, including the health check result and HTTP status code.
    /// </summary>
    /// <param name="Result">The health check result of the probe.</param>
    /// <param name="StatusCode">The HTTP status code returned by the endpoint, if available.</param>
    private sealed record EndpointProbeOutcome(
        HealthCheckResult Result,
        HttpStatusCode? StatusCode);
}