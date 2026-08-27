using Microsoft.Extensions.Diagnostics.HealthChecks;

using HealthDiagnostics.Mappers;
using HealthDiagnostics.Services; 
using SharedKernel.Modules;
using HealthDiagnostics.Controllers;

namespace HealthDiagnostics;

/// <summary>
/// Represents the Health Diagnostics module, which provides health diagnostics for the server and its modules.
/// </summary>
public sealed class HealthDiagnosticsModule : IModule
{
    public string Slug => "health-diagnostics";
    public string DisplayName => "Health Diagnostics";
    public string? Description => "Provides health diagnostics for the server and its modules.";
    public ModuleKind Kind => ModuleKind.SystemFeature;
    public string StaticFileUrlPrefix => "modules/health-diagnostics";

    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddControllers()
                .AddApplicationPart(typeof(HealthDiagnosticsController).Assembly);

        services.AddHttpClient("HealthProbeClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddSingleton<IHealthDiagnosticsMapper, HealthDiagnosticsMapper>();
        services.AddScoped<IHealthDiagnosticsServices, HealthDiagnosticsServices>();
    }

    /// <inheritdoc />
    public void RegisterHealthChecks(IHealthChecksBuilder healthChecksBuilder)
    {
        healthChecksBuilder.AddCheck(
            "health-diagnostics-self", 
            () => HealthCheckResult.Healthy("Diagnostics module active."), 
            tags: ["system", $"module:{Slug}"]);
    }
}