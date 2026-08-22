using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using GroceryStore;
using GroceryStore.Database.DbContexts;

using HealthDiagnostics;
using HealthDiagnostics.Controllers;
using ModuleCatalog;
using ModuleCatalog.Controllers;
using SystemSettings;
using SystemSettings.Controllers;

using SharedKernel.Modules;
using Server.Database.DbContexts;

namespace Server.Extensions;

public static class ServiceRegistration
{
    /// <summary>
    /// Registers all services required by the server and its modules.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder used to configure the application.</param>
    /// <returns>A read-only list of the registered modules.</returns>
    public static IReadOnlyList<IModule> AddServerServices(this WebApplicationBuilder builder)
    {
        RegisterLogging(builder);
        RegisterDatabases(builder.Services, builder.Configuration);

        var modules = RegisterModules(builder);

        RegisterHealthChecks(builder.Services, modules);
        RegisterApiServices(builder.Services);

        return modules;
    }

    /// <summary>
    /// Creates the available modules, registers them in DI, and executes their service configuration.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder used to configure the application.</param>
    /// <returns>An array of the registered modules.</returns>
    private static IReadOnlyList<IModule> RegisterModules(WebApplicationBuilder builder)
    {
        IModule[] modules =
        [
            new ModuleCatalogModule(),
            new GroceryStoreModule(),
            new HealthDiagnosticsModule(),
            new SystemSettingsModule()
        ];

        // Register modules in DI
        builder.Services.AddSingleton<IEnumerable<IModule>>(modules);
        builder.Services.AddSingleton<IReadOnlyList<IModule>>(modules);

        foreach (var module in modules)
        {
            module.ConfigureServices(builder.Services, builder.Configuration);
            builder.Services.AddSingleton(module);
        }

        return modules;
    }

    /// <summary>
    /// Adds global database health checks and module-specific health checks.
    /// </summary>
    /// <param name="services">The service collection used to configure the application.</param>
    /// <param name="modules">The list of registered modules.</param>
    private static void RegisterHealthChecks(IServiceCollection services, IEnumerable<IModule> modules)
    {
        var healthChecks = services.AddHealthChecks();

        healthChecks.AddDbContextCheck<GroceryStoreDbContext>(
            name: "grocerystore-db",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["db", "ready"]);

        healthChecks.AddDbContextCheck<ServerDbContext>(
            name: "server-db",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["db", "ready"]);

        foreach (var module in modules)
        {
            module.RegisterHealthChecks(healthChecks);
        }
    }

    /// <summary>
    /// Configures Serilog from the application's configuration sources.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder used to configure the application.</param>
    private static void RegisterLogging(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();
        });
    }

    /// <summary>
    /// Registers the MVC controllers across all module assemblies and OpenAPI services.
    /// </summary>
    /// <param name="services">The service collection used to configure the application.</param>
    private static void RegisterApiServices(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Stellt sicher, dass Controller aus allen Modul-Assemblies gefunden werden
        services.AddControllers(options =>
            {
                options.ReturnHttpNotAcceptable = true;
            })
                .AddApplicationPart(typeof(HealthDiagnosticsController).Assembly)
                .AddApplicationPart(typeof(ModuleCatalogController).Assembly)
                .AddApplicationPart(typeof(SystemSettingsController).Assembly);
    }

    /// <summary>
    /// Registers all Entity Framework database contexts.
    /// </summary>
    /// <param name="services">The service collection used to configure the application.</param>
    /// <param name="configuration">The configuration used to retrieve connection strings.</param>
    private static void RegisterDatabases(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GroceryStoreDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("GroceryStore")
                ?? throw new InvalidOperationException("The GroceryStore connection string is not configured.");
            options.UseSqlite(connectionString);
        });

        services.AddDbContext<ServerDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Server")
                ?? throw new InvalidOperationException("The Server connection string is not configured.");
            options.UseSqlite(connectionString);
        });
    }
}