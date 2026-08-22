using Serilog;
using Microsoft.EntityFrameworkCore;

using GroceryStore;
using GroceryStore.Database.DbContexts;

using ModuleCatalog;
using SystemSettings;
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
        var modules = RegisterModules(builder);

        RegisterHealthChecks(builder.Services, modules);
        RegisterLogging(builder);
        RegisterApiServices(builder.Services);
        RegisterDatabases(builder.Services, builder.Configuration);

        return modules;
    }

    /// <summary>
    /// Creates the available modules and lets each module register its dependencies.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder used to configure the application.</param>
    /// <returns>An array of the registered modules.</returns>
    private static IModule[] RegisterModules(WebApplicationBuilder builder)
    {
        var modules = new IModule[]
        {
            new ModuleCatalogModule(),
            new GroceryStoreModule(),
            new SystemSettingsModule()
        };

        foreach (var module in modules)
        {
            module.ConfigureServices(builder.Services, builder.Configuration);
            builder.Services.AddSingleton(module);
        }

        return modules;
    }

    /// <summary>
    /// Adds common health checks and the module-specific health checks.
    /// </summary>
    /// <param name="services">The service collection used to configure the application.</param>
    /// <param name="modules">The list of registered modules.</param>
    private static void RegisterHealthChecks(IServiceCollection services, IEnumerable<IModule> modules)
    {
        var healthChecks = services.AddHealthChecks();
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
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        });
    }

    /// <summary>
    /// Registers the MVC controllers and OpenAPI services.
    /// </summary>
    /// <param name="services">The service collection used to configure the application.</param>
    private static void RegisterApiServices(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddControllers();
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